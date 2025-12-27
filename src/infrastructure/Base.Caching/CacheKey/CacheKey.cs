using Base.Application.Contracts.Infrastructure.Caching;

namespace Base.Caching.CacheKey;

public class CacheKey(string key) : ICacheKey
{

    // IMPLEMENTATION OF ICacheKey
    public string Key { get; set; } = key;
    public int CacheTime { get; set; }
}
