using Base.Application.Contracts.Infrastructure.Caching;
using Base.Domain.Entities;
using Base.Domain.Options;
using Base.Infrastructure.Security;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;

namespace Base.Infrastructure.Caching;

public class CacheKeyService(IOptions<CacheConfig> cacheConfig) : ICacheKeyService
{
    // FIELDS
    private readonly CacheConfig _cacheConfig = cacheConfig.Value;

    // PROPERTY
    protected const string HashAlgorithm = "SHA1";

    /// <summary>
    /// CREATES A HASH FROM A COLLECTION OF INTEGER IDS FOR CACHE KEY GENERATION.
    /// </summary>
    protected virtual string CreateIdsHash(IEnumerable<int> ids)
    {
        var sortedIds = ids.Order().ToArray();

        if (sortedIds.Length == 0) return string.Empty;

        var identifiersString = string.Join(", ", sortedIds);

        return HashHelper.CreateHash(Encoding.UTF8.GetBytes(identifiersString), HashAlgorithm);
    }

    /// <summary>
    /// CONVERTS CACHE KEY PARAMETERS TO APPROPRIATE CACHE-FRIENDLY REPRESENTATIONS.
    /// </summary>
    protected virtual object CreateCacheKeyParameters(object parameter) => parameter switch
    {
        null => "null",
        IEnumerable<int> ids => CreateIdsHash(ids),
        IEnumerable<BaseEntity<int>> entities => CreateIdsHash(entities.Select(e => e.Id)),
        BaseEntity<int> entity => entity.Id,
        decimal value => value.ToString(CultureInfo.InvariantCulture),
        _ => parameter
    };

    /// <summary>
    /// PREPARES A CACHE KEY PREFIX BY FORMATTING IT WITH THE PROVIDED PARAMETERS.
    /// </summary>
    protected virtual string PrepareKeyPrefix(string prefix, params object[] prefixParameters)
    {
        return prefixParameters != null && prefixParameters.Length > 0
            ? string.Format(prefix, prefixParameters.Select(CreateCacheKeyParameters).ToArray())
            : prefix;
    }

    // IMPLEMENTATION OF ICACHEKEYSERVICE
    public ICacheKey PrepareKey(ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters)
    {
        ArgumentNullException.ThrowIfNull(cacheKeyFactory);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return cacheKeyFactory.Create(CreateCacheKeyParameters, key, cacheKeyParameters);
    }

    public ICacheKey PrepareKeyForDefaultCache(ICacheKeyFactory cacheKeyFactory, string key, params object[] cacheKeyParameters)
    {
        ArgumentNullException.ThrowIfNull(cacheKeyFactory);

        var cacheKey = cacheKeyFactory.Create(CreateCacheKeyParameters, key, cacheKeyParameters);

        if (cacheKey is CacheKey concreteKey)
        {
            concreteKey.CacheTime = _cacheConfig.DefaultCacheTimeInMinutes;
        }

        return cacheKey;
    }
}
