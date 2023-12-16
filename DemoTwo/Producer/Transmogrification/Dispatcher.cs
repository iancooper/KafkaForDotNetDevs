using Confluent.Kafka;

namespace Transmogrification;

public class Dispatcher(string topic, Dictionary<string, string> producerConfig) : IDisposable
{
    private readonly IProducer<string,string> _producer = new ProducerBuilder<string, string>(producerConfig).Build();

    public void Transmogrify(TransmogrificationSettings settings)
    {
        _producer.Produce(
            topic, 
            new Message<string, string>
            {
                Key = settings.Name, 
                Value = settings.Transformation
            }
        );
        _producer.Flush(TimeSpan.FromSeconds(10));
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}