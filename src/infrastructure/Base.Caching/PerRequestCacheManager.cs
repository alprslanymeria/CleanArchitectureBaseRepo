using Base.Application.Contracts.Infrastructure.Caching;
using Base.Caching.CacheKey;
using Base.Domain.Options.Caching;
using Microsoft.Extensions.Options;

namespace Base.Caching;

public class PerRequestCacheManager(
    
    IOptions<CacheConfig> cacheConfig,
    ICacheKeyFactory cacheKeyFactory
    
    ) : CacheKeyService(cacheConfig), IShortTermCacheManager

{
    // FIELDS
    protected readonly CacheKeyStore<object> Store = new();
    protected readonly ICacheKeyFactory CacheKeyFactory = cacheKeyFactory;

    // IMPLEMENTATION OF IShortTermCacheManager
    public virtual async Task<T> GetAsync<T>(Func<Task<T>> acquire, ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters)
    {
        var cacheKey = cacheKeyFactory.Create(CreateCacheKeyParameters, key, cacheKeyParameters).Key;

        if (Store.TryGetValue(cacheKey, out var data))
            return (T)data;

        var result = await acquire();

        if (result != null)
            Store.Add(cacheKey, result);

        return result;
    }

    public virtual void Remove(string cacheKey, params object[] cacheKeyParameters)
    {
        Store.Remove(PrepareKey(CacheKeyFactory, cacheKey, cacheKeyParameters).Key);
    }

    public virtual void RemoveByPrefix(string prefix, params object[] prefixParameters)
    {
        var keyPrefix = PrepareKeyPrefix(prefix, prefixParameters);
        Store.Prune(keyPrefix, out _);
    }
}