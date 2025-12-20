namespace Base.Domain.Options;

public class DistributedCacheConfig
{
    public const string Key = "DistributedCacheConfig";
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = default!;
    public string InstanceName { get; set; } = default!;
    public int PublishIntervalMs { get; set; }
}
