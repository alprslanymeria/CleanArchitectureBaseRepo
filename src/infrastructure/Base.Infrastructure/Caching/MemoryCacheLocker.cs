using Base.Application.Contracts.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace Base.Infrastructure.Caching;

public class MemoryCacheLocker(

    IMemoryCache memoryCache

    ) : ILocker
{

    // FIELDS
    protected readonly IMemoryCache _memoryCache = memoryCache;


    // UTILITIES
    protected virtual async Task<bool> RunAsync(string key, TimeSpan? expirationTime, Func<CancellationToken, Task> action, CancellationTokenSource cancellationTokenSource = default)
    {
        var started = false;

        try
        {
            if (!_memoryCache.TryGetValue(key, out Lazy<CancellationTokenSource> existing))
            {
                var lazy = _memoryCache.GetOrCreate(key, entry => new Lazy<CancellationTokenSource>(() =>
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
                _memoryCache.Remove(key);
        }

        return started;
    }


    // IMPLEMENTATION OF ILOCKER
    public virtual Task CancelTaskAsync(string key, TimeSpan expirationTime)
    {
        if (_memoryCache.TryGetValue(key, out Lazy<CancellationTokenSource> tokenSource))
            tokenSource.Value.Cancel();

        return Task.CompletedTask;
    }

    public Task<bool> IsTaskRunningAsync(string key)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
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
