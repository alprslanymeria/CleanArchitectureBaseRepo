namespace Base.Domain.Options.Caching;

public enum CacheType
{
    /// <summary>
    /// IN-MEMORY CACHE
    /// </summary>
    Memory,

    /// <summary>
    /// DISTRIBUTED CACHE
    /// </summary>
    Redis
}
