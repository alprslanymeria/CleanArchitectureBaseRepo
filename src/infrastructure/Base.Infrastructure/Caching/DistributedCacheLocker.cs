using Base.Application.Contracts.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Base.Infrastructure.Caching;

public class DistributedCacheLocker(

    IDistributedCache distributedCache

    ) : ILocker
{
    // FIELDS
    protected static readonly string _running = JsonSerializer.Serialize(TaskStatus.Running);
    protected readonly IDistributedCache _distributedCache = distributedCache;


    // IMPLEMENTATION OF ILOCKER
    public virtual async Task CancelTaskAsync(string key, TimeSpan expirationTime)
    {
        var status = await _distributedCache.GetStringAsync(key);

        if (!string.IsNullOrEmpty(status) && JsonSerializer.Deserialize<TaskStatus>(status) != TaskStatus.Canceled)
        {

            await _distributedCache.SetStringAsync(

                key,
                JsonSerializer.Serialize(TaskStatus.Canceled),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expirationTime });
        }

    }

    public virtual async Task<bool> IsTaskRunningAsync(string key)
    {
        return !string.IsNullOrEmpty(await _distributedCache.GetStringAsync(key));
    }

    public virtual async Task<bool> PerformActionWithLockAsync(string resource, TimeSpan expirationTime, Func<Task> action)
    {
        // ENSURE THAT LOCK IS ACQUIRED
        if (!string.IsNullOrEmpty(await _distributedCache.GetStringAsync(resource)))
            return false;

        try
        {
            await _distributedCache.SetStringAsync(resource, resource, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            });

            await action();

            return true;
        }
        finally
        {
            // RELEASE LOCK EVEN IF ACTION FAILS
            await _distributedCache.RemoveAsync(resource);
        }
    }

    public virtual async Task RunWithHeartbeatAsync(string key, TimeSpan expirationTime, TimeSpan heartbeatInterval, Func<CancellationToken, Task> action, CancellationTokenSource cancellationTokenSource = null)
    {
        if (!string.IsNullOrEmpty(await _distributedCache.GetStringAsync(key)))
            return;

        var tokenSource = cancellationTokenSource ?? new CancellationTokenSource();

        try
        {
            // run heartbeat early to minimize risk of multiple execution
            await _distributedCache.SetStringAsync(
                key,
                _running,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expirationTime },
                token: tokenSource.Token);

            await using var timer = new Timer(
                callback: _ =>
                {
                    try
                    {
                        tokenSource.Token.ThrowIfCancellationRequested();
                        var status = _distributedCache.GetString(key);
                        if (!string.IsNullOrEmpty(status) && JsonSerializer.Deserialize<TaskStatus>(status) ==
                            TaskStatus.Canceled)
                        {
                            tokenSource.Cancel();
                            return;
                        }

                        _distributedCache.SetString(
                            key,
                            _running,
                            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expirationTime });
                    }
                    catch (OperationCanceledException) { }
                },
                state: null,
                dueTime: 0,
                period: (int)heartbeatInterval.TotalMilliseconds);

            await action(tokenSource.Token);
        }
        catch (OperationCanceledException) { }
        finally
        {
            await _distributedCache.RemoveAsync(key);
        }
    }
}
