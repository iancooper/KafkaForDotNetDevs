using Confluent.Kafka;
using Spectre.Console;

namespace Transmogrifier;

public class MessagePump(string topic, Dictionary<string, string> consumerConfig)
{
    private readonly IConsumer<string, string> _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

    public async Task Run<TDataType>(
        Func<Message<string, string>, TDataType> translator,
        Action<TDataType> handler, 
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

                handler(dataType);
            }
        }
        catch(ConsumeException e)
        {
            AnsiConsole.WriteException(e);
        }
        catch (OperationCanceledException)
        {
            //Pump was cancelled, do nothing
        }
        finally
        {
            _consumer.Close();
        }
    }
}