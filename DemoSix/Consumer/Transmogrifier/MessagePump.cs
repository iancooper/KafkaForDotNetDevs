using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Spectre.Console;

namespace Transmogrifier;

public record HandleResult(bool Success);

public class MessagePump
{
    private readonly IConsumer<string, TransmogrificationSettings> _consumer;

    private readonly string _topic;

    public MessagePump(string topic, ConsumerConfig consumerConfig)
    {
        _topic = topic;
        _consumer = new ConsumerBuilder<string, TransmogrificationSettings>(consumerConfig)
            .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
            .SetLogHandler((_, lm) => Console.WriteLine($"Facility: {lm.Facility} Level: {lm.Level} Log: {lm.Message}"))
            .SetKeyDeserializer(Deserializers.Utf8)
            .SetValueDeserializer(new JsonDeserializer<TransmogrificationSettings>().AsSyncOverAsync())
            .SetPartitionsRevokedHandler((c, partitions) => c.Commit(partitions))
            .Build();
    }

    public async Task Run(
        Func<TransmogrificationSettings, HandleResult> handler, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _consumer.Subscribe(_topic);
            
            while (true) 
            {
                var consumeResult = _consumer.Consume(cancellationToken);

                if (consumeResult.IsPartitionEOF)
                {
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }
                
                var result = handler(consumeResult.Message.Value);
                if (result.Success)
                {
                    //We don't want to commit unless we have successfully handled the message
                    //Commit directly. Normally we would want to batch these up, but for the demo we will
                    //commit after each message
                    _consumer.Commit(consumeResult);
                }
            }
        }
        catch(ConsumeException e)
        {
            AnsiConsole.WriteException(e);
        }
        catch (OperationCanceledException)
        {
            //Pump was cancelled, exit
        }
        finally
        {
            _consumer.Close();
        }
    }
}