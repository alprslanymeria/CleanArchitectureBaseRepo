using Base.Application.Contracts.Infrastructure.Caching;
using Base.Caching;
using Base.Caching.CacheKey;
using Base.Caching.Locker;
using Base.Caching.Redis;

namespace Base.API.Extensions;

public static class CachingExtension
{
    public static IServiceCollection AddCaching(this IServiceCollection services)
    {

        // CACHING SERVICES
        services.AddTransient(typeof(ICacheKeyStore<>), typeof(CacheKeyStore<>));
        services.AddSingleton<ICacheKeyFactory, CacheKeyFactory>();
        services.AddSingleton<ICacheKeyManager, CacheKeyManager>();
        services.AddSingleton<ILocker, DistributedCacheLocker>();
        services.AddScoped<IShortTermCacheManager, PerRequestCacheManager>();
        services.AddSingleton<IRedisConnectionWrapper, RedisConnectionWrapper>();
        services.AddScoped<IStaticCacheManager, RedisCacheManager>();
        services.AddSingleton<ICacheKeyService, RedisCacheManager>();

        return services;
    }
}

