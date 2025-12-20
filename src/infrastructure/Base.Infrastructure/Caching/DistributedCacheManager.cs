using Base.Application.Contracts.Infrastructure.Caching;
using Base.Domain.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Base.Infrastructure.Caching;


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
    protected readonly ICacheKeyManager _localKeyManager = localKeyManager;
    protected readonly ICacheKeyStore<object> _keystore = keystore;
    protected readonly IDistributedCache _distributedCache = distributedCache;
    protected readonly ConcurrentDictionary<string, Lazy<Task<object>>> _ongoing = new();

    #region UTILITIES
    /// <summary>
    /// CLEARS ALL DATA FROM THE TRANSIENT STORE AND SINGLETON KEY MANAGER.
    /// </summary>
    protected virtual void ClearInstanceData()
    {
        _keystore.Clear();
        _localKeyManager.Clear();
    }

    /// <summary>
    /// REMOVES ALL CACHE ENTRIES MATCHING THE SPECIFIED PREFIX FROM THE TRANSIENT STORE AND SINGLETON KEY MANAGER.
    /// </summary>
    protected virtual IEnumerable<string> RemoveByPrefixInstanceData(string prefix, params object[] prefixParameters)
    {
        var keyPrefix = PrepareKeyPrefix(prefix, prefixParameters);

        _keystore.Prune(keyPrefix, out _);
        return _localKeyManager.RemoveByPrefix(keyPrefix);
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
        _keystore.Add(key, value);
        _localKeyManager.AddKey(key);
    }

    /// <summary>
    /// REMOVES A VALUE FROM THE TRANSIENT CACHE STORE AND UNREGISTERS THE KEY FROM THE SINGLETON KEY MANAGER.
    /// </summary>
    protected virtual void RemoveLocal(string key)
    {
        _keystore.Remove(key);
        _localKeyManager.RemoveKey(key);
    }

    /// <summary>
    /// ATTEMPTS TO RETRIEVE AND DESERIALIZE AN ITEM FROM THE DISTRIBUTED CACHE BY KEY.
    /// </summary>
    protected virtual async Task<(bool isSet, T? item)> TryGetItemAsync<T>(string key)
    {
        var json = await _distributedCache.GetStringAsync(key);

        return string.IsNullOrEmpty(json)
            ? (false, default)
            : (true, item: JsonSerializer.Deserialize<T>(json));
    }

    /// <summary>
    /// REMOVES A CACHE ENTRY FROM THE DISTRIBUTED CACHE AND OPTIONALLY FROM THE LOCAL INSTANCE.
    /// </summary>
    protected virtual async Task RemoveAsync(string key, bool removeFromInstance = true)
    {
        _ongoing.TryRemove(key, out _);

        await _distributedCache.RemoveAsync(key);

        if (!removeFromInstance) return;

        RemoveLocal(key);
    }
    #endregion


    // IMPLEMENTATION OF ISTATICCACHEMANAGER
    public abstract Task ClearAsync();

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual async Task<T> GetAsync<T>(ICacheKey key, Func<Task<T>> acquire)
    {
        if (_keystore.TryGetValue(key.Key, out var data))
            return (T)data;

        var lazy = _ongoing.GetOrAdd(key.Key, _ => new(async () => await acquire(), true));

        try
        {
            var (isSet, item) = await TryGetItemAsync<T>(key.Key);

            if (!isSet)
            {
                item = (T)await lazy.Value;

                if (key.CacheTime > 0 && item != null)
                {
                    await _distributedCache.SetStringAsync(
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
            _ongoing.TryRemove(key.Key, out _);
        }
    }

    public virtual async Task<T> GetAsync<T>(ICacheKey key, Func<T> acquire)
    {
        return await GetAsync(key, () => Task.FromResult(acquire()));
    }

    public virtual async Task<T> GetAsync<T>(ICacheKey key, T? defaultValue = default)
    {
        var value = await _distributedCache.GetStringAsync(key.Key);

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

        _ongoing.TryAdd(key.Key, lazy);
        try
        {
            var value = await lazy.Value;
            SetLocal(key.Key, value);

            var json = JsonSerializer.Serialize(data);
            await _distributedCache.SetStringAsync(key.Key, json, PrepareEntryOptions(key));
        }
        finally
        {
            _ongoing.TryRemove(key.Key, out _);
        }
    }

    public virtual async Task RemoveAsync(ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters)
    {
        await RemoveAsync(PrepareKey(cacheKeyFactory, key, cacheKeyParameters).Key);
    }

    public abstract Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters);
}
