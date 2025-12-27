using Base.Application.Contracts.Infrastructure.Caching;
using Base.Caching.CacheKey;
using Base.Domain.Options.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Base.Caching;

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
    protected readonly ICacheKeyManager LocalKeyManager = localKeyManager;    // FOR KEY TRACKING
    protected readonly IMemoryCache MemoryCache = memoryCache;
    protected readonly ICacheKeyFactory CacheKeyFactory = cacheKeyFactory;
    protected CancellationTokenSource ClearToken = new();
    protected bool Disposed;

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
                if (!MemoryCache.TryGetValue(key, out _))
                {
                    if (key is string cacheKey) LocalKeyManager.RemoveKey(cacheKey);
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

        options.AddExpirationToken(new CancellationChangeToken(ClearToken.Token));
        options.RegisterPostEvictionCallback(OnEviction);
        LocalKeyManager.AddKey(key.Key);

        return options;
    }

    /// <summary>
    /// CALLED WHEN THE CACHE MANAGER IS BEING DISPOSED.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (Disposed)
            return;

        if (disposing) ClearToken.Dispose();

        Disposed = true;
    }
    #endregion


    // IMPLEMENTATION OF IStaticCacheManager
    public virtual async Task ClearAsync()
    {
        await ClearToken.CancelAsync();
        ClearToken.Dispose();
        ClearToken = new CancellationTokenSource();
        LocalKeyManager.Clear();
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

        var task = MemoryCache.GetOrCreate(
            key.Key,
            entry =>
            {
                entry.SetOptions(PrepareEntryOptions((CacheKey.CacheKey)key));
                return new Lazy<Task<T>>(acquire, true);
            });

        try
        {
            var data = await task!.Value;

            //if a cached function return null, remove it from the cache
            if (data == null)
                await RemoveAsync(CacheKeyFactory, key.Key);

            return data;
        }
        catch (Exception ex)
        {
            //if a cached function throws an exception, remove it from the cache
            await RemoveAsync(CacheKeyFactory, key.Key);

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
        var value = MemoryCache.Get<Lazy<Task<T>>>(key.Key)?.Value;

        try
        {
            return value != null ? await value : defaultValue;
        }
        catch
        {
            //if a cached function throws an exception, remove it from the cache
            await RemoveAsync(CacheKeyFactory, key.Key);

            throw;
        }
    }

    public virtual async Task<object> GetAsync(ICacheKey key)
    {
        var entry = MemoryCache.Get(key.Key);
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
            await RemoveAsync(CacheKeyFactory, key.Key);

            throw;
        }
    }

    public virtual async Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters)
    {
        var deletePrefix = PrepareKeyPrefix(prefix, prefixParameters);

        foreach (var key in LocalKeyManager.RemoveByPrefix(deletePrefix))
            MemoryCache.Remove(key);
    }

    public Task SetAsync<T>(ICacheKey key, T data)
    {
        if (data != null && (key?.CacheTime ?? 0) > 0)
            MemoryCache.Set(
                key.Key,
                new Lazy<Task<T>>(() => Task.FromResult(data), true),
                PrepareEntryOptions((CacheKey.CacheKey)key));

        return Task.CompletedTask;
    }

    public virtual Task RemoveAsync(ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters)
    {
        var cacheKey = PrepareKey(cacheKeyFactory, key, cacheKeyParameters).Key;
        MemoryCache.Remove(cacheKey);
        LocalKeyManager.RemoveKey(cacheKey);

        return Task.CompletedTask;
    }
}
