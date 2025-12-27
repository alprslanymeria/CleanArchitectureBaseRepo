using System.Collections.Concurrent;
using System.Text.Json;
using Base.Application.Contracts.Infrastructure.Caching;
using Base.Caching.CacheKey;
using Base.Domain.Options.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Base.Caching;


// singleton ICacheKeyManager (localKeyManager)
    // transient ICacheKeyStore , scoped olarak çalışır

// singleton IDistributedCache (distributedCache)
// transient ICacheKeyStore (keystore)


public abstract class DistributedCacheManager(

    IOptions<CacheConfig> cacheConfig,
    ICacheKeyManager localKeyManager,
    IDistributedCache distributedCache,
    ICacheKeyStore<object> keystore
    
    ) : CacheKeyService(cacheConfig), IStaticCacheManager, ICacheKeyFactoryRemover
{
    // FIELDS
    protected readonly ICacheKeyManager LocalKeyManager = localKeyManager;
    protected readonly ICacheKeyStore<object> Keystore = keystore;
    protected readonly IDistributedCache DistributedCache = distributedCache;
    protected readonly ConcurrentDictionary<string, Lazy<Task<object>>> Ongoing = new();

    #region UTILITIES
    /// <summary>
    /// CLEARS ALL DATA FROM THE TRANSIENT STORE AND SINGLETON KEY MANAGER.
    /// </summary>
    protected virtual void ClearInstanceData()
    {
        Keystore.Clear();
        LocalKeyManager.Clear();
    }

    /// <summary>
    /// REMOVES ALL CACHE ENTRIES MATCHING THE SPECIFIED PREFIX FROM THE TRANSIENT STORE AND SINGLETON KEY MANAGER.
    /// </summary>
    protected virtual IEnumerable<string> RemoveByPrefixInstanceData(string prefix, params object[] prefixParameters)
    {
        var keyPrefix = PrepareKeyPrefix(prefix, prefixParameters);

               Keystore.Prune(keyPrefix, out _);
        return LocalKeyManager.RemoveByPrefix(keyPrefix);
    }

    /// <summary>
    /// PREPARES DISTRIBUTED CACHE ENTRY OPTIONS WITH ABSOLUTE EXPIRATION BASED ON THE CACHE KEY'S CACHE TIME.
    /// </summary>
    protected virtual DistributedCacheEntryOptions PrepareEntryOptions(ICacheKey key)
    {
        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(key.CacheTime)
        };
    }

    /// <summary>
    /// ADDS A VALUE TO THE LOCAL TRANSIENT STORE AND REGISTERS THE KEY WITH THE SINGLETON KEY MANAGER.
    /// </summary>
    protected virtual void SetLocal(string key, object value)
    {
        Keystore.Add(key, value);
        LocalKeyManager.AddKey(key);
    }

    /// <summary>
    /// REMOVES A VALUE FROM THE TRANSIENT CACHE STORE AND UNREGISTERS THE KEY FROM THE SINGLETON KEY MANAGER.
    /// </summary>
    protected virtual void RemoveLocal(string key)
    {
        Keystore.Remove(key);
        LocalKeyManager.RemoveKey(key);
    }

    /// <summary>
    /// ATTEMPTS TO RETRIEVE AND DESERIALIZE AN ITEM FROM THE DISTRIBUTED CACHE BY KEY.
    /// </summary>
    protected virtual async Task<(bool isSet, T? item)> TryGetItemAsync<T>(string key)
    {
        var json = await DistributedCache.GetStringAsync(key);

        return string.IsNullOrEmpty(json)
            ? (false, default)
            : (true, item: JsonSerializer.Deserialize<T>(json));
    }

    /// <summary>
    /// REMOVES A CACHE ENTRY FROM THE DISTRIBUTED CACHE AND OPTIONALLY FROM THE LOCAL INSTANCE.
    /// </summary>
    protected virtual async Task RemoveAsync(string key, bool removeFromInstance = true)
    {
        Ongoing.TryRemove(key, out _);

        await DistributedCache.RemoveAsync(key);

        if (!removeFromInstance) return;

        RemoveLocal(key);
    }
    #endregion


    // IMPLEMENTATION OF IStaticCacheManager
    public abstract Task ClearAsync();

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual async Task<T> GetAsync<T>(ICacheKey key, Func<Task<T>> acquire)
    {
        if (Keystore.TryGetValue(key.Key, out var data))
            return (T)data;

        var lazy = Ongoing.GetOrAdd(key.Key, _ => new(async () => await acquire(), true));

        try
        {
            var (isSet, item) = await TryGetItemAsync<T>(key.Key);

            if (!isSet)
            {
                item = (T)await lazy.Value;

                if (key.CacheTime > 0 && item != null)
                {
                    await DistributedCache.SetStringAsync(
                        key.Key,
                        JsonSerializer.Serialize(item),
                        PrepareEntryOptions(key));
                }
            }

            if (item == null)
                return item;

            SetLocal(key.Key, item);
            return item;
        }
        finally
        {
            Ongoing.TryRemove(key.Key, out _);
        }
    }

    public virtual async Task<T> GetAsync<T>(ICacheKey key, Func<T> acquire)
    {
        return await GetAsync(key, () => Task.FromResult(acquire()));
    }

    public virtual async Task<T> GetAsync<T>(ICacheKey key, T? defaultValue = default)
    {
        var value = await DistributedCache.GetStringAsync(key.Key);

        return value != null
            ? JsonSerializer.Deserialize<T>(value)
            : defaultValue;
    }

    public virtual async Task<object> GetAsync(ICacheKey key)
    {
        return await GetAsync<object>(key);
    }
    public virtual async Task SetAsync<T>(ICacheKey key, T data)
    {
        if (data is null || key?.CacheTime is null or <= 0) return;

        var lazy = new Lazy<Task<object>>(() => Task.FromResult<object>(data!), isThreadSafe: true);

        Ongoing.TryAdd(key.Key, lazy);
        try
        {
            var value = await lazy.Value;
            SetLocal(key.Key, value);

            var json = JsonSerializer.Serialize(data);
            await DistributedCache.SetStringAsync(key.Key, json, PrepareEntryOptions(key));
        }
        finally
        {
            Ongoing.TryRemove(key.Key, out _);
        }
    }

    public virtual async Task RemoveAsync(ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters)
    {
        await RemoveAsync(PrepareKey(cacheKeyFactory, key, cacheKeyParameters).Key);
    }

    public abstract Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters);
}
