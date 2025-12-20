using Base.Application.Contracts.Infrastructure.Caching;
using Base.Domain.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Base.Infrastructure.Caching;

// singleton ICacheKeyManager (localKeyManager)
// transient ICacheKeyStore , scoped olarak çalışır

// singleton IMemoryCache (memoryCache)

public class MemoryCacheManager(

    IOptions<CacheConfig> cacheConfig,
    ICacheKeyManager localKeyManager,
    IMemoryCache memoryCache,
    ICacheKeyFactory cacheKeyFactory

    ) : CacheKeyService(cacheConfig), IStaticCacheManager, ICacheKeyFactoryRemover
{
    // FIELDS
    protected readonly ICacheKeyManager _localKeyManager = localKeyManager;    // FOR KEY TRACKING
    protected readonly IMemoryCache _memoryCache = memoryCache;
    protected readonly ICacheKeyFactory _cacheKeyFactory = cacheKeyFactory;
    protected CancellationTokenSource _clearToken = new();
    protected bool _disposed;

    #region UTILITIES

    /// <summary>
    /// CALLED WHEN A CACHE ENTRY IS EVICTED; REMOVES THE KEY FROM THE LOCAL KEY MANAGER IF THE ENTRY IS NO LONGER PRESENT AND THE EVICTION REASON IS NOT REMOVED, REPLACED, OR TOKENEXPIRED.
    /// </summary>
    protected virtual void OnEviction(object key, object value, EvictionReason reason, object state)
    {
        switch (reason)
        {
            case EvictionReason.Removed:
            case EvictionReason.Replaced:
            case EvictionReason.TokenExpired:
                break;
            default:
                if (!_memoryCache.TryGetValue(key, out _))
                {
                    if (key is string cacheKey) _localKeyManager.RemoveKey(cacheKey);
                }
                break;
        }
    }

    /// <summary>
    /// PREPARES THE MEMORY CACHE ENTRY OPTIONS FOR A GIVEN CACHE KEY.
    /// </summary>
    protected virtual MemoryCacheEntryOptions PrepareEntryOptions(ICacheKey key)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(key.CacheTime)
        };

        options.AddExpirationToken(new CancellationChangeToken(_clearToken.Token));
        options.RegisterPostEvictionCallback(OnEviction);
        _localKeyManager.AddKey(key.Key);

        return options;
    }

    /// <summary>
    /// CALLED WHEN THE CACHE MANAGER IS BEING DISPOSED.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing) _clearToken.Dispose();

        _disposed = true;
    }
    #endregion


    // IMPLEMENTATION OF ISTATICCACHEMANAGER
    public virtual async Task ClearAsync()
    {
        await _clearToken.CancelAsync();
        _clearToken.Dispose();
        _clearToken = new CancellationTokenSource();
        _localKeyManager.Clear();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual async Task<T> GetAsync<T>(ICacheKey key, Func<Task<T>> acquire)
    {
        if ((key?.CacheTime ?? 0) <= 0)
            return await acquire();

        var task = _memoryCache.GetOrCreate(
            key.Key,
            entry =>
            {
                entry.SetOptions(PrepareEntryOptions((CacheKey)key));
                return new Lazy<Task<T>>(acquire, true);
            });

        try
        {
            var data = await task!.Value;

            //if a cached function return null, remove it from the cache
            if (data == null)
                await RemoveAsync(_cacheKeyFactory, key.Key);

            return data;
        }
        catch (Exception ex)
        {
            //if a cached function throws an exception, remove it from the cache
            await RemoveAsync(_cacheKeyFactory, key.Key);

            if (ex is NullReferenceException)
                return default;

            throw;
        }
    }

    public virtual async Task<T> GetAsync<T>(ICacheKey key, Func<T> acquire)
    {
        return await GetAsync(key, () => Task.FromResult(acquire()));
    }

    public virtual async Task<T> GetAsync<T>(ICacheKey key, T defaultValue = default)
    {
        var value = _memoryCache.Get<Lazy<Task<T>>>(key.Key)?.Value;

        try
        {
            return value != null ? await value : defaultValue;
        }
        catch
        {
            //if a cached function throws an exception, remove it from the cache
            await RemoveAsync(_cacheKeyFactory, key.Key);

            throw;
        }
    }

    public virtual async Task<object> GetAsync(ICacheKey key)
    {
        var entry = _memoryCache.Get(key.Key);
        if (entry == null)
            return null;
        try
        {
            if (entry.GetType().GetProperty("Value")?.GetValue(entry) is not Task task)
                return null;

            await task;

            return task.GetType().GetProperty("Result")!.GetValue(task);
        }
        catch
        {
            //if a cached function throws an exception, remove it from the cache
            await RemoveAsync(_cacheKeyFactory, key.Key);

            throw;
        }
    }

    public virtual async Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters)
    {
        var deletePrefix = PrepareKeyPrefix(prefix, prefixParameters);

        foreach (var key in _localKeyManager.RemoveByPrefix(deletePrefix))
            _memoryCache.Remove(key);
    }

    public Task SetAsync<T>(ICacheKey key, T data)
    {
        if (data != null && (key?.CacheTime ?? 0) > 0)
            _memoryCache.Set(
                key.Key,
                new Lazy<Task<T>>(() => Task.FromResult(data), true),
                PrepareEntryOptions((CacheKey)key));

        return Task.CompletedTask;
    }

    public virtual Task RemoveAsync(ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters)
    {
        var cacheKey = PrepareKey(cacheKeyFactory, key, cacheKeyParameters).Key;
        _memoryCache.Remove(cacheKey);
        _localKeyManager.RemoveKey(cacheKey);

        return Task.CompletedTask;
    }
}
