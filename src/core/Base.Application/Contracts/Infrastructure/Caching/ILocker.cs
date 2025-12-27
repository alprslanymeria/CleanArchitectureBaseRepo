namespace Base.Application.Contracts.Infrastructure.Caching;

public interface ILocker
{
    /// <summary>
    /// EXECUTES AN ACTION WITH A DISTRIBUTED LOCK TO ENSURE EXCLUSIVITY
    /// </summary>
    Task<bool> PerformActionWithLockAsync(string resource, TimeSpan expirationTime, Func<Task> action);
    /// <summary>
    /// RUNS A TASK WITH A HEARTBEAT TO KEEP IT ALIVE
    /// </summary>
    Task RunWithHeartbeatAsync(string key, TimeSpan expirationTime, TimeSpan heartbeatInterval, Func<CancellationToken, Task> action, CancellationTokenSource cancellationTokenSource = default);
    /// <summary>
    /// CANCELS A RUNNING TASK
    /// </summary>
    Task CancelTaskAsync(string key, TimeSpan expirationTime);
    /// <summary>
    /// CHECKS IF A TASK IS CURRENTLY RUNNING FOR A GIVEN KEY
    /// </summary>
    Task<bool> IsTaskRunningAsync(string key);
}
