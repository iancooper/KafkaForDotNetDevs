using Confluent.Kafka;

namespace Transmogrification;

public class Dispatcher(string topic, ProducerConfig producerConfig, IOutbox outbox) : IDisposable
{
    private readonly IProducer<string,string> _producer = new ProducerBuilder<string, string>(producerConfig)
         .SetLogHandler((_, message) =>
            Console.WriteLine($"Facility: {message.Facility}-{message.Level} Message: {message.Message}"))
        .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}. Is Fatal: {e.IsFatal}"))
        .Build();

    public void Transmogrify(TransmogrificationSettings settings)
    {
        try
        {
            _producer.InitTransactions(TimeSpan.FromSeconds(10));
            _producer.BeginTransaction();
            
            Action<DeliveryReport<string,string>> handler = report =>
            {
                //we use an outbox to be able to retry sending the message if it fails
                //we don't show a sweeper in this example, but it would be a separate process that would
                //periodically check the outbox for messages that haven't been persisted yet and retry sending them
                outbox.MarkStatus(report.Topic, report.Key, report.Partition, report.Status, report.Timestamp);
            };
            
            var message = new Message<string, string>
            {
                Key = settings.Name, 
                Value = settings.Transformation
            };
            
            
            //We could send multiple messages as part of the transaction, but we only send one
            _producer.Produce(topic, message, handler);
            
            //If the outbox add fails, then the transaction will be aborted and the message won't be sent
            outbox.Add(topic, message.Key, message);
            
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