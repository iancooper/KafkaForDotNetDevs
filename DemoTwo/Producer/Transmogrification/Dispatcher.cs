using Confluent.Kafka;

namespace Transmogrification;

public class Dispatcher(string topic, Dictionary<string, string> producerConfig)
{
    public void Transmogrify(TransmogrificationSettings settings)
    {
        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
        producer.Produce(topic, new Message<string, string> { Key = settings.Name, Value = settings.Transformation });
        producer.Flush(TimeSpan.FromSeconds(10));
    }
}