using Confluent.Kafka;
using Spectre.Console;

namespace Transmogrifier;

public record HandleResult(bool Success);

public class MessagePump(string topic, ConsumerConfig consumerConfig)
{
    private readonly IConsumer<string, string> _consumer = new ConsumerBuilder<string, string>(consumerConfig)
        .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
        .SetLogHandler((_, lm) => Console.WriteLine($"Facility: {lm.Facility} Level: {lm.Level} Log: {lm.Message}"))
        .SetPartitionsRevokedHandler((c, partitions) => c.Commit(partitions))
        .Build();

    public async Task Run<TDataType>(
        Func<Message<string, string>, TDataType> translator,
        Func<TDataType, HandleResult> handler, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _consumer.Subscribe(topic);
            
            while (true)
            {
                var consumeResult = _consumer.Consume(cancellationToken);

                if (consumeResult.IsPartitionEOF)
                {
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }
                
                var dataType = translator(consumeResult.Message);

                var result = handler(dataType);
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