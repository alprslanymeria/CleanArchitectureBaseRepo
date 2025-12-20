using Base.Application.Contracts.Infrastructure.Caching;
using Base.Domain.Options;
using Microsoft.Extensions.Options;

namespace Base.Infrastructure.Caching;

public class PerRequestCacheManager(

    IOptions<CacheConfig> cacheConfig,
    ICacheKeyFactory cacheKeyFactory

    ) : CacheKeyService(cacheConfig), IShortTermCacheManager

{
    // FIELDS
    protected readonly CacheKeyStore<object> _store = new();
    protected readonly ICacheKeyFactory _cacheKeyFactory = cacheKeyFactory;

    // IMPLEMENTATION OF ISHORTTERMCACHEMANAGER
    public virtual async Task<T> GetAsync<T>(Func<Task<T>> acquire, ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters)
    {
        var cacheKey = cacheKeyFactory.Create(CreateCacheKeyParameters, key, cacheKeyParameters).Key;

        if (_store.TryGetValue(cacheKey, out var data))
            return (T)data;

        var result = await acquire();

        if (result != null)
            _store.Add(cacheKey, result);

        return result;
    }

    public virtual void Remove(string cacheKey, params object[] cacheKeyParameters)
    {
        _store.Remove(PrepareKey(_cacheKeyFactory, cacheKey, cacheKeyParameters).Key);
    }

    public virtual void RemoveByPrefix(string prefix, params object[] prefixParameters)
    {
        var keyPrefix = PrepareKeyPrefix(prefix, prefixParameters);
        _store.Prune(keyPrefix, out _);
    }
}