using Base.Application.Contracts.Infrastructure.Caching;

namespace Base.Caching.CacheKey;

public class CacheKeyManager(ICacheKeyStore<byte> store) : ICacheKeyManager
{
    // FIELDS
    private readonly ICacheKeyStore<byte> _keystore = store;
    public IEnumerable<string> Keys => _keystore.Keys;

    // IMPLEMENTATION OF ICacheKeyManager
    public virtual void AddKey(string key) => _keystore.Add(key, 0);

    public virtual void Clear() => _keystore.Clear();

    public virtual void RemoveKey(string key) => _keystore.Remove(key);

    public virtual IEnumerable<string> RemoveByPrefix(string prefix)
    {
        if (!_keystore.Prune(prefix, out var subtree) || subtree?.Keys == null)
            return [];

        return subtree.Keys;
    }
}
