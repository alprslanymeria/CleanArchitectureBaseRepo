using Base.Application.Contracts.Infrastructure.Caching;
using Base.Domain.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net;

namespace Base.Infrastructure.Caching;

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
    protected readonly IRedisConnectionWrapper _connectionWrapper = redisConnectionWrapper;
    protected readonly DistributedCacheConfig _distributedCacheConfig = distributedCacheConfig.Value;

    #region UTILITIES
    protected virtual async Task<IEnumerable<RedisKey>> GetKeysAsync(EndPoint endPoint, string prefix = null)
    {
        return await (await _connectionWrapper.GetServerAsync(endPoint))
            .KeysAsync((await _connectionWrapper.GetDatabaseAsync()).Database, string.IsNullOrEmpty(prefix) ? null : $"{prefix}*")
            .ToListAsync();
    }
    #endregion

    // IMPLEMENTATION OF ABSTRACT METHODS
    public override async Task ClearAsync()
    {
        await _connectionWrapper.FlushDatabaseAsync();

        ClearInstanceData();
    }

    public override async Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters)
    {
        prefix = PrepareKeyPrefix(prefix, prefixParameters);

        var db = await _connectionWrapper.GetDatabaseAsync();

        var instanceName = _distributedCacheConfig.InstanceName ?? string.Empty;

        foreach (var endPoint in await _connectionWrapper.GetEndPointsAsync())
        {
            var keys = await GetKeysAsync(endPoint, instanceName + prefix);
            await db.KeyDeleteAsync(keys.ToArray());
        }

        RemoveByPrefixInstanceData(prefix);
    }
}
