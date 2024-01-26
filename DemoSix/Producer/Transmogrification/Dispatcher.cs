using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace Transmogrification;

public class Dispatcher : IDisposable
{
    private readonly IProducer<string,TransmogrificationSettings> _producer;
    
    private readonly ISchemaRegistryClient _schemaRegistryClient;
    private readonly string _topic;
    private readonly IOutbox _outbox;

    public Dispatcher(string topic, ProducerConfig producerConfig, SchemaRegistryConfig schemaRegistryConfig, IOutbox outbox)
    {
        _topic = topic;
        _outbox = outbox;
        _schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
        var serializerConfig = new JsonSerializerConfig { BufferBytes = 100 };
        
        _producer = new ProducerBuilder<string, TransmogrificationSettings>(producerConfig)
            .SetLogHandler((_, message) =>
                Console.WriteLine($"Facility: {message.Facility}-{message.Level} Message: {message.Message}"))
            .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}. Is Fatal: {e.IsFatal}"))
            .SetValueSerializer(new JsonSerializer<TransmogrificationSettings>(_schemaRegistryClient, serializerConfig))
            .Build();
    }

    public void Transmogrify(TransmogrificationSettings settings)
    {
        try
        {
            _producer.InitTransactions(TimeSpan.FromSeconds(10));
            _producer.BeginTransaction();
            
            Action<DeliveryReport<string,TransmogrificationSettings>> handler = report =>
            {
                //we use an outbox to be able to retry sending the message if it fails
                //we don't show a sweeper in this example, but it would be a separate process that would
                //periodically check the outbox for messages that haven't been persisted yet and retry sending them
                _outbox.MarkStatus(report.Topic, report.Key, report.Partition, report.Status, report.Timestamp);
            };
            
            var serializer = new JsonSerializer<TransmogrificationSettings>(_schemaRegistryClient).AsSyncOverAsync();
            
            var message = new Message<string, TransmogrificationSettings>
            {
                Key = settings.Name, 
                Value = settings
            };
            
            
            //We could send multiple messages as part of the transaction, but we only send one
            _producer.Produce(_topic, message, handler);
            
            //If the outbox add fails, then the transaction will be aborted and the message won't be sent
            _outbox.Add(_topic, message.Key, message);
            
            _producer.CommitTransaction(TimeSpan.FromSeconds(10));
        }
        catch (ProduceException<string, string> e)
        {
            Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            _producer.AbortTransaction(TimeSpan.FromSeconds(10));
        }
        catch (KafkaException e)
        {
            Console.WriteLine($"Fatal error: {e.Error.Reason}");
            _producer.AbortTransaction(TimeSpan.FromSeconds(10));
        }
        finally
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}