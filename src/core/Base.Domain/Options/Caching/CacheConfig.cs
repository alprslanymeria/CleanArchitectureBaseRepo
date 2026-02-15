namespace Base.Domain.Options.Caching;

public class CacheConfig 
{

    public const string Key = "CacheConfig";

    /// <summary>
    /// CACHE TYPE
    /// </summary>
    public CacheType CacheType { get; set; } = CacheType.Memory;

    public bool Enable { get; set; } = true;

    /// <summary>
    /// DEFAULT CACHE TIME IN MINUTES
    /// </summary>
    public int DefaultCacheTimeInMinutes { get; set; }
}
