using Base.Application.Contracts.Infrastructure.Caching;

namespace Base.Caching.CacheKey;

public class CacheKeyManager(ICacheKeyStore<byte> store) : ICacheKeyManager
{
    // FIELDS
    public IEnumerable<string> Keys => store.Keys;

    // IMPLEMENTATION OF ICacheKeyManager
    public virtual void AddKey(string key) => store.Add(key, 0);

    public virtual void Clear() => store.Clear();

    public virtual void RemoveKey(string key) => store.Remove(key);

    public virtual IEnumerable<string> RemoveByPrefix(string prefix)
    {
        if (!store.Prune(prefix, out var subtree) || subtree?.Keys == null)
            return [];

        return subtree.Keys;
    }
}
