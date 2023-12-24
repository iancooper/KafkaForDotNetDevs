using Confluent.Kafka;

namespace Transmogrification;

public class AsyncDispatcher(string topic, ProducerConfig producerConfig, IOutbox outbox) : IDisposable
{
    private readonly IProducer<string,string> _producer = new ProducerBuilder<string, string>(producerConfig)
         .SetLogHandler((_, message) =>
            Console.WriteLine($"Facility: {message.Facility}-{message.Level} Message: {message.Message}"))
        .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}. Is Fatal: {e.IsFatal}"))
        .Build();

    public async Task Transmogrify(TransmogrificationSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = settings.Name, 
                Value = settings.Transformation
            };
            
            outbox.Add(topic, message.Key, message);
            
            //This will not complete until the message is sent; as such throughput will be lower than Produce
            var report = await _producer.ProduceAsync(topic, message, cancellationToken); 
            
            //we use an outbox to be able to retry sending the message if it fails
            ////we don't show a sweeper in this example, but it would be a separate process that would
            //periodically check the outbox for messages that haven't been persisted yet and retry sending them
            outbox.MarkAsPersisted(report.Topic, report.Key, report.Partition, report.Status, report.Timestamp);
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