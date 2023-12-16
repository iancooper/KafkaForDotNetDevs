using System.Text.Json;
using Confluent.Kafka;
using Spectre.Console;

namespace Transmogrifier;

public class MessagePump(string topic, Dictionary<string, string> consumerConfig) 
{
    private readonly IConsumer<string, string> _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

    public async Task Run(Action<Transmogrification> handler, CancellationToken cancellationToken = default)
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
                
                var transmogrification = TranslateMessage(consumeResult.Message);

                handler(transmogrification);
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
    
    private Transmogrification TranslateMessage(Message<string, string> message)
    {
        return new Transmogrification(message.Key, message.Value);

    }
}