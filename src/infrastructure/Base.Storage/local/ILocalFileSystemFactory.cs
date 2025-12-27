namespace Base.Storage.local;

/// <summary>
/// FACTORY INTERFACE FOR LOCAL FILE SYSTEM OPERATIONS
/// </summary>
public interface ILocalFileSystemFactory
{
    /// <summary>
    /// GETS THE ROOT PATH FOR FILE STORAGE
    /// </summary>
    string GetRootPath();

    /// <summary>
    /// ENSURES THE ROOT DIRECTORY EXISTS
    /// </summary>
    void EnsureRootDirectoryExists();

    /// <summary>
    /// CREATES A FILE STREAM FOR WRITING
    /// </summary>
    Stream CreateWriteStream(string fullPath);

    /// <summary>
    /// CREATES A FILE STREAM FOR READING
    /// </summary>
    Stream CreateReadStream(string fullPath);
}
