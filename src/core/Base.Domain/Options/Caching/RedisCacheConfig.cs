namespace Base.Domain.Options.Caching;

public class RedisCacheConfig
{
    public const string Key = "RedisCache";

    /// <summary>
    /// CONNECTION STRING FOR THE DISTRIBUTED CACHE
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// NAME OF THE REDIS CACHE INSTANCE
    /// </summary>
    public string InstanceName { get; set; } = null!;

    /// <summary>
    /// PUBLISH INTERVAL FOR CACHE INVALIDATION
    /// </summary>
    public int PublishIntervalMs { get; set; }
}
