using Base.Application.Contracts.Infrastructure.Storage;
using Base.Domain.Options.Storage;
using Microsoft.Extensions.Options;

namespace Base.Storage.local;

/// <summary>
/// LOCAL FILE SYSTEM STORAGE PROVIDER IMPLEMENTATION
/// </summary>
public class LocalFileStorageProvider(

    ILocalFileSystemFactory fileSystemFactory,
    IOptions<LocalStorageConfig> config

    ) : IStorageProvider
{
    // FIELDS
    private readonly LocalStorageConfig _config = config.Value;
    private readonly string _rootPath = fileSystemFactory.GetRootPath();
    private bool _disposed;

    #region UTILS
    /// <summary>
    /// GET FULL PATH FROM RELATIVE PATH
    /// </summary>
    private string GetFullPath(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_rootPath, normalized);
    }

    /// <summary>
    /// BUILD RELATIVE PATH FROM FILE NAME AND FOLDER
    /// </summary>
    private static string BuildRelativePath(string fileName, string? folderPath)
        => string.IsNullOrEmpty(folderPath) 
            ? fileName 
            : Path.Combine(folderPath.Trim('/').Replace('/', Path.DirectorySeparatorChar), fileName);

    /// <summary>
    /// VALIDATE FILE EXTENSION
    /// </summary>
    private void ValidateFile(string fileName)
    {
        if (_config.AllowedExtensions.Length <= 0) return;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!_config.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"File extension '{extension}' is not allowed.");
        }
    }
    #endregion

    // IMPLEMENTATION OF IStorageProvider
    public string ProviderName => "LocalFileStorage";

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folderPath = null, CancellationToken cancellationToken = default)
    {
        ValidateFile(fileName);

        var relativePath = BuildRelativePath(fileName, folderPath);
        var fullPath = GetFullPath(relativePath);

        await using var fileStream = fileSystemFactory.CreateWriteStream(fullPath);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return GetPublicUrl(relativePath);
    }

    public async Task<string> UploadAsync(byte[] data, string fileName, string contentType, string? folderPath = null, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(data);
        return await UploadAsync(stream, fileName, contentType, folderPath, cancellationToken);
    }

    public async Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(filePath);

        await using var fileStream = fileSystemFactory.CreateReadStream(fullPath);
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    public async Task DownloadToStreamAsync(string filePath, Stream destination, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(filePath);

        await using var fileStream = fileSystemFactory.CreateReadStream(fullPath);
        await fileStream.CopyToAsync(destination, cancellationToken);
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(filePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(filePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public string GetPublicUrl(string filePath)
    {
        return !string.IsNullOrEmpty(_config.BaseUrl)
            ? $"{_config.BaseUrl.TrimEnd('/')}/{filePath.Replace('\\', '/')}"
            : $"/files/{filePath.Replace('\\', '/')}";
    }

    public Task<string> GetSignedUrlAsync(string filePath, int expirationMinutes = 60, CancellationToken cancellationToken = default)
    {
        // LOCAL STORAGE DOESN'T SUPPORT SIGNED URLS, RETURN REGULAR URL
        return Task.FromResult(GetPublicUrl(filePath));
    }

    public Task<IEnumerable<string>> ListFilesAsync(string? folderPath = null, CancellationToken cancellationToken = default)
    {
        var searchPath = string.IsNullOrEmpty(folderPath)
            ? _rootPath
            : Path.Combine(_rootPath, folderPath);

        if (!Directory.Exists(searchPath))
        {
            return Task.FromResult<IEnumerable<string>>([]);
        }

        var files = Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(_rootPath, f).Replace('\\', '/'))
            .ToList();

        return Task.FromResult<IEnumerable<string>>(files);
    }

    public Task CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        var sourceFullPath = GetFullPath(sourcePath);
        var destFullPath = GetFullPath(destinationPath);

        if (!File.Exists(sourceFullPath))
        {
            throw new FileNotFoundException($"Source file not found: {sourcePath}", sourcePath);
        }

        var destDirectory = Path.GetDirectoryName(destFullPath);
        if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
        {
            Directory.CreateDirectory(destDirectory);
        }

        File.Copy(sourceFullPath, destFullPath, overwrite: true);
        return Task.CompletedTask;
    }

    public Task MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        var sourceFullPath = GetFullPath(sourcePath);
        var destFullPath = GetFullPath(destinationPath);

        if (!File.Exists(sourceFullPath))
        {
            throw new FileNotFoundException($"Source file not found: {sourcePath}", sourcePath);
        }

        var destDirectory = Path.GetDirectoryName(destFullPath);
        if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
        {
            Directory.CreateDirectory(destDirectory);
        }

        File.Move(sourceFullPath, destFullPath, overwrite: true);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
