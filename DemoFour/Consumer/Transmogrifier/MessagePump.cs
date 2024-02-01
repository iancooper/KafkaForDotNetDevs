using Confluent.Kafka;
using Spectre.Console;

namespace Transmogrifier;

public record HandleResult(bool Success);

public class MessagePump(string topic, ConsumerConfig consumerConfig)
{
    private readonly IConsumer<string, string> _consumer = new ConsumerBuilder<string, string>(consumerConfig)
        .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
        .SetLogHandler((_, lm) => Console.WriteLine($"Facility: {lm.Facility} Level: {lm.Level} Log: {lm.Message}"))
        .SetPartitionsRevokedHandler((c, partions) => partions.ForEach(p => c.StoreOffset(p)))
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
                    //_consumer.Commit(consumeResult); would commit manually, but with EnableAutoOffsetStore disabled,
                    //we can instead just manually store "done" offsets for a background thread to commit
                    _consumer.StoreOffset(consumeResult);
                    AnsiConsole.WriteLine("Stored Offset: Topic: " + consumeResult.TopicPartitionOffset.Topic 
                            + " Partition: " + consumeResult.TopicPartitionOffset.Partition 
                            + " Offset: " + consumeResult.TopicPartitionOffset.Offset);
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