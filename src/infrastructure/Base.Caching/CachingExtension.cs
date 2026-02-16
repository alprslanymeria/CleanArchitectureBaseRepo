using Base.Application.Contracts.Infrastructure.Caching;
using Base.Caching.CacheKey;
using Base.Caching.Locker;
using Base.Caching.Redis;
using Base.Domain.Options.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Base.Caching;

public static class CachingExtension
{
    public static IServiceCollection AddCachingServicesExt(this IServiceCollection services, IConfiguration configuration)
    {
        // LOAD CACHE CONFIGURATION AND VALIDATE
        var cacheConfig = configuration
            .GetRequiredSection(CacheConfig.Key)
            .Get<CacheConfig>()
            ?? throw new InvalidOperationException("CacheConfig is missing in appsettings");

        // CONFIGURATION BINDINGS
        services.Configure<CacheConfig>(configuration.GetSection(CacheConfig.Key));
        services.Configure<RedisCacheConfig>(configuration.GetSection(RedisCacheConfig.Key));


        // COMMON CACHING SERVICES
        services.AddTransient(typeof(ICacheKeyStore<>), typeof(CacheKeyStore<>));
        services.AddSingleton<ICacheKeyFactory, CacheKeyFactory>();
        services.AddSingleton<ICacheKeyManager, CacheKeyManager>();
        services.AddScoped<IShortTermCacheManager, PerRequestCacheManager>();

        // REGISTRATION BASED ON CACHE TYPE
        switch (cacheConfig.CacheType)
        {
            case CacheType.Redis:
                AddRedisCaching(services, configuration);
                break;

            case CacheType.Memory:
                AddMemoryCaching(services);
                break;

            default:
                throw new NotSupportedException($"Cache type '{cacheConfig.CacheType}' is not supported.");
        }

        return services;
    }

    private static void AddRedisCaching(IServiceCollection services, IConfiguration configuration)
    {

        // LOAD REDIS CACHE CONFIGURATION AND VALIDATE
        var redisConfig = configuration
            .GetRequiredSection(RedisCacheConfig.Key)
            .Get<RedisCacheConfig>()
            ?? throw new InvalidOperationException("RedisCacheConfig is missing in appsettings");

        // CHECK CONNECTION STRING
        if (string.IsNullOrWhiteSpace(redisConfig.ConnectionString))
        {
            throw new InvalidOperationException("Redis ConnectionString is required");
        }

        // REDIS CONNECTION MULTIPLEXER (LAZY - CONNECTS ON FIRST USE)
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConfig.ConnectionString));

        // REDIS SPECIFIC SERVICES
        services.AddSingleton<IRedisConnectionWrapper, RedisConnectionWrapper>();
        services.AddSingleton<ILocker, DistributedCacheLocker>();
        services.AddSingleton<IStaticCacheManager, RedisCacheManager>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig.ConnectionString;
            options.InstanceName = redisConfig.InstanceName;
        });
    }

    private static void AddMemoryCaching(IServiceCollection services)
    {
        // MEMORY CACHE SERVICES
        services.AddMemoryCache();
        services.AddSingleton<ILocker, MemoryCacheLocker>();
        services.AddSingleton<IStaticCacheManager, MemoryCacheManager>();
    }
}
