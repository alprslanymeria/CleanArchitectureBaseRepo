namespace Base.Application.Contracts.Infrastructure.Caching;

/// <summary>
/// MANAGES CACHE KEYS FOR TRACKING AND INVALIDATION OPERATIONS.
/// </summary>
public interface ICacheKeyManager
{
    /// <summary>
    /// GETS ALL REGISTERED CACHE KEYS.
    /// </summary>
    IEnumerable<string> Keys { get; }

    /// <summary>
    /// ADDS A NEW KEY TO THE CACHE KEY COLLECTION.
    /// </summary>
    void AddKey(string key);

    /// <summary>
    /// REMOVES A SPECIFIC KEY FROM THE CACHE KEY COLLECTION.
    /// </summary>
    void RemoveKey(string key);

    /// <summary>
    /// CLEARS ALL KEYS FROM THE CACHE KEY COLLECTION.
    /// </summary>
    void Clear();

    /// <summary>
    /// REMOVES ALL KEYS THAT START WITH THE SPECIFIED PREFIX.
    /// </summary>
    IEnumerable<string> RemoveByPrefix(string prefix);
}
