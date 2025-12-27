using System.Text.Json;
using Base.Application.Contracts.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace Base.Caching.Locker;

public class DistributedCacheLocker(

    IDistributedCache distributedCache

    ) : ILocker
{
    // FIELDS
    protected static readonly string Running = JsonSerializer.Serialize(TaskStatus.Running);
    protected readonly IDistributedCache DistributedCache = distributedCache;


    // IMPLEMENTATION OF ILocker
    public virtual async Task CancelTaskAsync(string key, TimeSpan expirationTime)
    {
        var status = await DistributedCache.GetStringAsync(key);

        if (!string.IsNullOrEmpty(status) && JsonSerializer.Deserialize<TaskStatus>(status) != TaskStatus.Canceled)
        {

            await DistributedCache.SetStringAsync(

                key,
                JsonSerializer.Serialize(TaskStatus.Canceled),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expirationTime });
        }
            
    }

    public virtual async Task<bool> IsTaskRunningAsync(string key)
    {
        return !string.IsNullOrEmpty(await DistributedCache.GetStringAsync(key));
    }

    public virtual async Task<bool> PerformActionWithLockAsync(string resource, TimeSpan expirationTime, Func<Task> action)
    {
        // ENSURE THAT LOCK IS ACQUIRED
        if (!string.IsNullOrEmpty(await DistributedCache.GetStringAsync(resource)))
            return false;

        try
        {
            await DistributedCache.SetStringAsync(resource, resource, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            });

            await action();

            return true;
        }
        finally
        {
            // RELEASE LOCK EVEN IF ACTION FAILS
            await DistributedCache.RemoveAsync(resource);
        }
    }

    public virtual async Task RunWithHeartbeatAsync(string key, TimeSpan expirationTime, TimeSpan heartbeatInterval, Func<CancellationToken, Task> action, CancellationTokenSource cancellationTokenSource = null)
    {
        if (!string.IsNullOrEmpty(await DistributedCache.GetStringAsync(key)))
            return;

        var tokenSource = cancellationTokenSource ?? new CancellationTokenSource();

        try
        {
            // run heartbeat early to minimize risk of multiple execution
            await DistributedCache.SetStringAsync(
                key,
                Running,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expirationTime },
                token: tokenSource.Token);

            await using var timer = new Timer(
                callback: _ =>
                {
                    try
                    {
                        tokenSource.Token.ThrowIfCancellationRequested();
                        var status = DistributedCache.GetString(key);
                        if (!string.IsNullOrEmpty(status) && JsonSerializer.Deserialize<TaskStatus>(status) ==
                            TaskStatus.Canceled)
                        {
                            tokenSource.Cancel();
                            return;
                        }

                        DistributedCache.SetString(
                            key,
                            Running,
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
            await DistributedCache.RemoveAsync(key);
        }
    }
}
