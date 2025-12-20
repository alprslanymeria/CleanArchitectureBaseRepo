namespace Base.Application.Contracts.Infrastructure.Caching;

/// <summary>
/// PROVIDES CACHE KEY GENERATION AND PREPARATION SERVICES.
/// </summary>
public interface ICacheKeyService
{
    /// <summary>
    /// PREPARES A CACHE KEY USING THE SPECIFIED FACTORY AND PARAMETERS.
    /// </summary>
    ICacheKey PrepareKey(ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters);

    /// <summary>
    /// PREPARES A CACHE KEY FOR DEFAULT CACHE USING THE SPECIFIED FACTORY AND PARAMETERS.
    /// </summary>
    ICacheKey PrepareKeyForDefaultCache(ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters);
}
