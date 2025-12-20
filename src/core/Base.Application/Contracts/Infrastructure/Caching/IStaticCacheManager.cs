namespace Base.Application.Contracts.Infrastructure.Caching;

/// <summary>
/// PROVIDES STATIC CACHE MANAGEMENT OPERATIONS FOR STORING, RETRIEVING, AND INVALIDATING CACHED DATA.
/// </summary>
public interface IStaticCacheManager : IDisposable, ICacheKeyService
{
    /// <summary>
    /// RETRIEVES CACHED DATA OR EXECUTES ASYNC FACTORY FUNCTION TO ACQUIRE AND CACHE THE VALUE.
    /// </summary>
    Task<T> GetAsync<T>(ICacheKey key, Func<Task<T>> acquire);

    /// <summary>
    /// RETRIEVES CACHED DATA OR EXECUTES SYNC FACTORY FUNCTION TO ACQUIRE AND CACHE THE VALUE.
    /// </summary>
    Task<T> GetAsync<T>(ICacheKey key, Func<T> acquire);

    /// <summary>
    /// RETRIEVES CACHED DATA OR RETURNS THE SPECIFIED DEFAULT VALUE IF NOT FOUND.
    /// </summary>
    Task<T> GetAsync<T>(ICacheKey key, T defaultValue = default);

    /// <summary>
    /// RETRIEVES CACHED DATA AS AN UNTYPED OBJECT BY THE SPECIFIED CACHE KEY.
    /// </summary>
    Task<object> GetAsync(ICacheKey key);

    /// <summary>
    /// STORES THE SPECIFIED DATA IN CACHE WITH THE GIVEN CACHE KEY.
    /// </summary>
    Task SetAsync<T>(ICacheKey key, T data);

    /// <summary>
    /// REMOVES ALL CACHED ENTRIES MATCHING THE SPECIFIED PREFIX AND PARAMETERS.
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters);

    /// <summary>
    /// CLEARS ALL CACHED DATA FROM THE CACHE STORE.
    /// </summary>
    Task ClearAsync();
}

/// <summary>
/// PROVIDES CACHE REMOVAL OPERATIONS USING A PREDEFINED CACHE KEY INSTANCE.
/// </summary>
public interface ICacheKeyRemover
{
    /// <summary>
    /// REMOVES CACHED ENTRY BY THE SPECIFIED CACHE KEY AND OPTIONAL PARAMETERS.
    /// </summary>
    Task RemoveAsync(ICacheKey cacheKey, params object[] cacheKeyParameters);
}

/// <summary>
/// PROVIDES CACHE REMOVAL OPERATIONS USING A CACHE KEY FACTORY FOR DYNAMIC KEY GENERATION.
/// </summary>
public interface ICacheKeyFactoryRemover
{
    /// <summary>
    /// REMOVES CACHED ENTRY USING FACTORY-GENERATED KEY WITH SPECIFIED PARAMETERS.
    /// </summary>
    Task RemoveAsync(ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters);
}
