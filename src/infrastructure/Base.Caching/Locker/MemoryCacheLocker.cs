using Base.Application.Contracts.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace Base.Caching.Locker;

public class MemoryCacheLocker(

    IMemoryCache memoryCache

    ) : ILocker
{

    // FIELDS
    protected readonly IMemoryCache MemoryCache = memoryCache;


    // UTILITIES
    protected virtual async Task<bool> RunAsync(string key, TimeSpan? expirationTime, Func<CancellationToken, Task> action, CancellationTokenSource cancellationTokenSource = default)
    {
        var started = false;

        try
        {
            if (!MemoryCache.TryGetValue(key, out Lazy<CancellationTokenSource> existing))
            {
                var lazy = MemoryCache.GetOrCreate(key, entry => new Lazy<CancellationTokenSource>(() =>
                {
                    entry.AbsoluteExpirationRelativeToNow = expirationTime;
                    entry.SetPriority(CacheItemPriority.NeverRemove);
                    started = true;
                    return cancellationTokenSource ?? new CancellationTokenSource();

                }, true));

                var tokenSource = lazy?.Value;

                if (tokenSource != null && started)
                    await action(tokenSource.Token);
            }
        }
        catch (OperationCanceledException) 
        { 
        
        }
        finally
        {
            if (started)
                MemoryCache.Remove(key);
        }

        return started;
    }


    // IMPLEMENTATION OF ILocker
    public virtual Task CancelTaskAsync(string key, TimeSpan expirationTime)
    {
        if (MemoryCache.TryGetValue(key, out Lazy<CancellationTokenSource> tokenSource))
            tokenSource.Value.Cancel();

        return Task.CompletedTask;
    }

    public Task<bool> IsTaskRunningAsync(string key)
    {
        return Task.FromResult(MemoryCache.TryGetValue(key, out _));
    }

    public virtual async Task<bool> PerformActionWithLockAsync(string resource, TimeSpan expirationTime, Func<Task> action)
    {
        return await RunAsync(resource, expirationTime, _ => action());
    }

    public virtual async Task RunWithHeartbeatAsync(string key, TimeSpan expirationTime, TimeSpan heartbeatInterval, Func<CancellationToken, Task> action, CancellationTokenSource cancellationTokenSource = null)
    {
        await RunAsync(key, null, action, cancellationTokenSource);
    }
}
