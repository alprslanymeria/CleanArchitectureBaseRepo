using Base.Application.Contracts.Infrastructure.Storage;
using Base.Domain.Options.Storage;
using Microsoft.Extensions.Options;

namespace Base.Storage;

/// <summary>
/// MAIN STORAGE SERVICE INTERFACE THAT DELEGATES TO THE CONFIGURED STORAGE PROVIDER
/// </summary>
public class StorageService(

    IStorageProviderFactory providerFactory,
    IOptions<StorageConfig> config

    ) : IStorageService

{

    // FIELDS
    private readonly StorageConfig _config = config.Value;
    private bool _disposed;

    // IMPLEMENTATION OF IStorageService
    public StorageType CurrentStorageType => _config.StorageType;
    public IStorageProvider Provider { get; } = providerFactory.GetDefaultProvider();

    public string ProviderName => Provider.ProviderName;

    public Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folderPath = null, CancellationToken cancellationToken = default)
        => Provider.UploadAsync(stream, fileName, contentType, folderPath, cancellationToken);

    public Task<string> UploadAsync(byte[] data, string fileName, string contentType, string? folderPath = null, CancellationToken cancellationToken = default)
        => Provider.UploadAsync(data, fileName, contentType, folderPath, cancellationToken);

    public Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
        => Provider.DownloadAsync(filePath, cancellationToken);

    public Task DownloadToStreamAsync(string filePath, Stream destination, CancellationToken cancellationToken = default)
        => Provider.DownloadToStreamAsync(filePath, destination, cancellationToken);

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        => Provider.DeleteAsync(filePath, cancellationToken);

    public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
        => Provider.ExistsAsync(filePath, cancellationToken);

    public string GetPublicUrl(string filePath)
        => Provider.GetPublicUrl(filePath);

    public Task<string> GetSignedUrlAsync(string filePath, int expirationMinutes = 60, CancellationToken cancellationToken = default)
        => Provider.GetSignedUrlAsync(filePath, expirationMinutes, cancellationToken);

    public Task<IEnumerable<string>> ListFilesAsync(string? folderPath = null, CancellationToken cancellationToken = default)
        => Provider.ListFilesAsync(folderPath, cancellationToken);

    public Task CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
        => Provider.CopyAsync(sourcePath, destinationPath, cancellationToken);

    public Task MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
        => Provider.MoveAsync(sourcePath, destinationPath, cancellationToken);

    public void Dispose()
    {
        if (_disposed) return;

        Provider?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
