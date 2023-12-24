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
            Action<DeliveryReport<string,string>> handler = report =>
            {
                //we use an outbox to be able to retry sending the message if it fails
                //we don't show a sweeper in this example, but it would be a separate process that would
                //periodically check the outbox for messages that haven't been persisted yet and retry sending them
                outbox.MarkAsPersisted(report.Topic, report.Key);
            };
            
            var message = new Message<string, string>
            {
                Key = settings.Name, 
                Value = settings.Transformation
            };
            
            outbox.Add(topic, message.Key, message);
            
            //This runs asynchronously - it won't block the thread, instead it will call the handler when the message is sent
            _producer.Produce(topic, message, handler);
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