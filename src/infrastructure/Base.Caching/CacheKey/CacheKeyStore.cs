using Base.Application.Contracts.Infrastructure.Caching;

namespace Base.Caching.CacheKey;

public class CacheKeyStore<TValue> : ICacheKeyStore<TValue>
{
    // PROPS
    private readonly Dictionary<string, TValue> _items = [];

    // IMPLEMENTATION OF ICacheKeyStore<TValue>
    public IEnumerable<string> Keys => _items.Keys;

    public void Add(string key, TValue value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _items[key] = value;
    }

    public void Clear()
    {
        _items.Clear();
    }

    public void Remove(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _items.Remove(key);
    }

    public bool TryGetValue(string key, out TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _items.TryGetValue(key, out value);
    }

    public IEnumerable<KeyValuePair<string, TValue>> Search(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        return _items.Where(kv => kv.Key.StartsWith(prefix, StringComparison.Ordinal));
    }

    public IEnumerable<string> RemoveByPrefix(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        var keysToRemove = FindKeysByPrefix(prefix);

        foreach (var key in keysToRemove)
        {
            _items.Remove(key);
        }

        return keysToRemove;
    }

    public bool Prune(string prefix, out ICacheKeyStore<TValue> subCollection)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        var keysToRemove = FindKeysByPrefix(prefix);

        if (keysToRemove.Count == 0)
        {
            subCollection = default;
            return false;
        }

        subCollection = ExtractSubCollection(keysToRemove);
        return true;
    }

    private List<string> FindKeysByPrefix(string prefix)
    {
        return _items.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .ToList();
    }

    private CacheKeyStore<TValue> ExtractSubCollection(List<string> keys)
    {
        var subCollection = new CacheKeyStore<TValue>();

        foreach (var key in keys)
        {
            subCollection._items[key] = _items[key];
            _items.Remove(key);
        }

        return subCollection;
    }
}