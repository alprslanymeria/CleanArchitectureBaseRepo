namespace Base.Application.Contracts.Infrastructure.Caching;

/// <summary>
/// DEFINES THE CONTRACT FOR CACHE KEY CONFIGURATION.
/// </summary>
public interface ICacheKey
{
    /// <summary>
    /// GETS OR SETS THE UNIQUE IDENTIFIER FOR THE CACHE ENTRY.
    /// </summary>
    string Key { get; set; }

    /// <summary>
    /// GETS OR SETS THE CACHE DURATION IN MINUTES.
    /// </summary>
    int CacheTime { get; set; }
}
