namespace Base.Domain.Options;

public class CacheConfig
{
    public const string Key = "CacheConfig";
    public int DefaultCacheTimeInMinutes { get; set; }
}
