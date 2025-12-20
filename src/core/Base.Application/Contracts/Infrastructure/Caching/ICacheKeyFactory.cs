namespace Base.Application.Contracts.Infrastructure.Caching;

/// <summary>
/// FACTORY INTERFACE FOR CREATING CACHE KEYS.
/// </summary>
public interface ICacheKeyFactory
{
    /// <summary>
    /// CREATES A CACHE KEY WITH ADDITIONAL PARAMETERS (OPTIONAL).
    /// </summary>
    ICacheKey Create(Func<object, object> createCacheKeyParameters, string key, params object[] keyObjects);
}
