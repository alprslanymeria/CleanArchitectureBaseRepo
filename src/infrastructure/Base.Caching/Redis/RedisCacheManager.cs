using System.Net;
using Base.Application.Contracts.Infrastructure.Caching;
using Base.Domain.Options.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Base.Caching.Redis;

public class RedisCacheManager(

    IOptions<CacheConfig> cacheConfig,
    IOptions<DistributedCacheConfig> distributedCacheConfig,
    ICacheKeyManager localKeyManager,
    IDistributedCache distributedCache,
    ICacheKeyStore<object> store,
    IRedisConnectionWrapper redisConnectionWrapper

    ) : DistributedCacheManager(cacheConfig, localKeyManager, distributedCache, store)

{

    // FIELDS
    protected readonly IRedisConnectionWrapper ConnectionWrapper = redisConnectionWrapper;
    protected readonly DistributedCacheConfig DistributedCacheConfig = distributedCacheConfig.Value;

    #region UTILITIES
    protected virtual async Task<IEnumerable<RedisKey>> GetKeysAsync(EndPoint endPoint, string prefix = null)
    {
        return await (await ConnectionWrapper.GetServerAsync(endPoint))
            .KeysAsync((await ConnectionWrapper.GetDatabaseAsync()).Database, string.IsNullOrEmpty(prefix) ? null : $"{prefix}*")
            .ToListAsync();
    }
    #endregion

    // IMPLEMENTATION OF ABSTRACT METHODS
    public override async Task ClearAsync()
    {
        await ConnectionWrapper.FlushDatabaseAsync();

        ClearInstanceData();
    }

    public override async Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters)
    {
        prefix = PrepareKeyPrefix(prefix, prefixParameters);

        var db = await ConnectionWrapper.GetDatabaseAsync();

        var instanceName = DistributedCacheConfig.InstanceName ?? string.Empty;

        foreach (var endPoint in await ConnectionWrapper.GetEndPointsAsync())
        {
            var keys = await GetKeysAsync(endPoint, instanceName + prefix);
            await db.KeyDeleteAsync(keys.ToArray());
        }

        RemoveByPrefixInstanceData(prefix);
    }
}
