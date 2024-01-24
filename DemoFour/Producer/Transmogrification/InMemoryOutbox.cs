using System.Text;
using Confluent.Kafka;

namespace Transmogrification;

public record Entry(string Topic, string Key, Message<string, string> Value, PersistenceStatus Status, Partition Partition, Timestamp Timestamp) { }

public interface IOutbox
{
    void Add(string topic, string key, Message<string, string> value);
    void MarkStatus(string topic, string key, Partition partition, PersistenceStatus status, Timestamp timestamp);
    IEnumerable<Entry> FindNotPersisted(string topic);
    IEnumerable<Entry> FindPersisted(string topic);
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
        _entries[topic].Add(key, new Entry(topic, key, value, PersistenceStatus.NotPersisted, default, default));
    }

    public void MarkStatus(string topic, string key, Partition partition, PersistenceStatus status, Timestamp timestamp)
    {
        _entries[topic][key] = _entries[topic][key] with { Status = status, Partition = partition, Timestamp = timestamp };
    }
    
    public IEnumerable<Entry> FindNotPersisted(string topic)
    {
        foreach (var entry in _entries[topic].Values.Where(e => e.Status != PersistenceStatus.Persisted))
        {
            yield return entry;
        }
    }

    public IEnumerable<Entry> FindPersisted(string topic)
    {
        foreach (var entry in _entries[topic].Values.Where(e => e.Status == PersistenceStatus.Persisted))
        {
            yield return entry;
        }
    }
}