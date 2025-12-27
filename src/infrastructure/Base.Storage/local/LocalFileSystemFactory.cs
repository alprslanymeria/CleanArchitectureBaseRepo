using Base.Domain.Options.Storage;
using Microsoft.Extensions.Options;

namespace Base.Storage.local;

/// <summary>
/// FACTORY IMPLEMENTATION FOR LOCAL FILE SYSTEM OPERATIONS
/// </summary>
public class LocalFileSystemFactory : ILocalFileSystemFactory
{
    // FIELDS
    private readonly LocalStorageConfig _config;
    private readonly string _rootPath;

    public LocalFileSystemFactory(IOptions<LocalStorageConfig> config)
    {
        _config = config.Value;
        _rootPath = Path.GetFullPath(_config.RootPath);
    }


    // IMPLEMENTATION OF ILocalFileSystemFactory
    public string GetRootPath() => _rootPath;

    public void EnsureRootDirectoryExists()
    {
        if (_config.CreateDirectoriesIfNotExist && !Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
        }
    }

    public Stream CreateWriteStream(string fullPath)
    {
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
    }

    public Stream CreateReadStream(string fullPath)
    {
        return File.Exists(fullPath) ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read) : throw new FileNotFoundException($"File not found: {fullPath}", fullPath);
    }
}
