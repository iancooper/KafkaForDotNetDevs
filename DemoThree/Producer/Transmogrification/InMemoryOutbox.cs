using System.Text;
using Confluent.Kafka;

namespace Transmogrification;

public record Entry(string topic, string Key, Message<string, string> Value, PersistenceStatus Status) {}

public interface IOutbox
{
    void Add(string topic, string key, Message<string, string> value);
    void MarkAsPersisted(string topic, string key);
    IEnumerable<Entry> FindNotPersisted(string topic);
}

public class InMemoryOutbox : IOutbox
{
    private readonly Dictionary<string, Dictionary<string, Entry>> _entries = new();

    public InMemoryOutbox(IEnumerable<string> topics)
    {
        foreach (var topic in topics)
        {
            _entries.Add(topic, new Dictionary<string, Entry>());
        }
    }

    public void Add(string topic, string key, Message<string, string> value)
    {
        _entries[topic].Add(key, new Entry(topic, key, value, PersistenceStatus.NotPersisted));
    }
    
    public void MarkAsPersisted(string topic, string key)
    {
        _entries[topic][key] = _entries[topic][key] with { Status = PersistenceStatus.Persisted };
    }
    
    public IEnumerable<Entry> FindNotPersisted(string topic)
    {
        foreach (var entry in _entries[topic].Values.Where(e => e.Status != PersistenceStatus.Persisted))
        {
            yield return entry;
        }
    }
}