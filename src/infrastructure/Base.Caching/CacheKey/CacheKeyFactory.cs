using Base.Application.Contracts.Infrastructure.Caching;
using Base.Domain.Options.Caching;
using Microsoft.Extensions.Options;

namespace Base.Caching.CacheKey;

public class CacheKeyFactory(
    
    IOptions<CacheConfig> cacheConfig
    
    ) : ICacheKeyFactory

{

    // FIELDS
    private readonly int _defaultCacheTime = cacheConfig.Value.DefaultCacheTimeInMinutes;

    // IMPLEMENTATION OF ICacheKeyFactory
    public ICacheKey Create(Func<object, object> createCacheKeyParameters, string key, params object[] keyObjects)
    {
        var cacheKey = new CacheKey(key) { CacheTime = _defaultCacheTime };

        if (keyObjects.Length == 0) return cacheKey;

        cacheKey.Key = string.Format(cacheKey.Key, keyObjects.Select(createCacheKeyParameters).ToArray());

        return cacheKey;
    }
}
