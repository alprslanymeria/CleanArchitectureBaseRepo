namespace Base.Application.Contracts.Infrastructure.Caching;

/// <summary>
/// PROVIDES SHORT-TERM CACHING OPERATIONS WITH KEY-BASED RETRIEVAL AND REMOVAL CAPABILITIES.
/// </summary>
public interface IShortTermCacheManager : ICacheKeyService
{
    /// <summary>
    /// RETRIEVES A CACHED ITEM BY KEY OR ACQUIRES AND CACHES IT IF NOT PRESENT.
    /// </summary>
    Task<T> GetAsync<T>(Func<Task<T>> acquire, ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters);

    /// <summary>
    /// REMOVES A SPECIFIC CACHED ITEM BY ITS KEY.
    /// </summary>
    void Remove(string cacheKey, params object[] cacheKeyParameters);

    /// <summary>
    /// REMOVES ALL CACHED ITEMS MATCHING THE SPECIFIED PREFIX.
    /// </summary>
    void RemoveByPrefix(string prefix, params object[] prefixParameters);
}
