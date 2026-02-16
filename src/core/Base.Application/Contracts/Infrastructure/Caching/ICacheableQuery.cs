namespace Base.Application.Contracts.Infrastructure.Caching;

public interface ICacheableQuery
{
    ICacheKey GetCacheKey(ICacheKeyFactory keyFactory);
}
