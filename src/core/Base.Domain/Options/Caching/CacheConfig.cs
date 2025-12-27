namespace Base.Domain.Options.Caching;

public class CacheConfig 
{

    public const string Key = "CacheConfig";

    /// <summary>
    /// DEFAULT CACHE TIME IN MINUTES
    /// </summary>
    public int DefaultCacheTimeInMinutes { get; set; }
}
