namespace Base.Application.Contracts.Infrastructure.Caching;

/// <summary>
/// PREFIX BASED CACHE KEY STORE FOR CACHE INVALIDATION PURPOSES
/// </summary>
public interface ICacheKeyStore<TValue>
{
    /// <summary>
    /// GETS ALL KEYS STORED IN THE CACHE KEY STORE.
    /// </summary>
    IEnumerable<string> Keys { get; }

    /// <summary>
    /// ATTEMPTS TO GET THE VALUE ASSOCIATED WITH THE SPECIFIED KEY.
    /// </summary>
    bool TryGetValue(string key, out TValue value);

    /// <summary>
    /// ADDS A KEY-VALUE PAIR TO THE CACHE KEY STORE.
    /// </summary>
    void Add(string key, TValue value);

    /// <summary>
    /// REMOVES ALL KEYS AND VALUES FROM THE CACHE KEY STORE.
    /// </summary>
    void Clear();

    /// <summary>
    /// SEARCHES FOR ALL KEY-VALUE PAIRS MATCHING THE SPECIFIED PREFIX.
    /// </summary>
    IEnumerable<KeyValuePair<string, TValue>> Search(string prefix);

    /// <summary>
    /// REMOVES THE SPECIFIED KEY FROM THE CACHE KEY STORE.
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// REMOVES ALL KEYS MATCHING THE SPECIFIED PREFIX AND RETURNS THE REMOVED KEYS.
    /// </summary>
    IEnumerable<string> RemoveByPrefix(string prefix);

    /// <summary>
    /// EXTRACTS A SUB-COLLECTION OF KEYS MATCHING THE PREFIX AND REMOVES THEM FROM THE CURRENT STORE.
    /// </summary>
    bool Prune(string prefix, out ICacheKeyStore<TValue> subCollection);
}
