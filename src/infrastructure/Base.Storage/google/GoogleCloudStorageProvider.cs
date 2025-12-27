using System.Net;
using Base.Application.Contracts.Infrastructure.Storage;
using Base.Domain.Options.Storage;
using Google;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace Base.Storage.google;

/// <summary>
/// GOOGLE CLOUD STORAGE PROVIDER IMPLEMENTATION
/// </summary>
public class GoogleCloudStorageProvider(

    IGoogleCloudStorageClientFactory clientFactory,
    IOptions<GoogleCloudStorageConfig> config
    
    ) : IStorageProvider
{
    // FIELDS
    private readonly StorageClient _storageClient = clientFactory.CreateStorageClient();
    private readonly UrlSigner _urlSigner = clientFactory.CreateUrlSigner();
    private readonly GoogleCloudStorageConfig _config = config.Value;
    private bool _disposed;

    #region UTILS
    /// <summary>
    /// CREATE FILE NAME AND PATH
    /// </summary>
    private static string BuildObjectName(string fileName, string? folderPath) 
        => string.IsNullOrEmpty(folderPath) ? fileName : $"{folderPath.Trim('/')}/{fileName}";
    #endregion

    // IMPLEMENTATION OF IStorageProvider
    public string ProviderName => "GoogleCloudStorage";

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folderPath = null, CancellationToken cancellationToken = default)
    {
        var objectName = BuildObjectName(fileName, folderPath);

        var uploadObjectOptions = new UploadObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead };

        var uploadedObject = await _storageClient.UploadObjectAsync(
            _config.BucketName,
            objectName,
            contentType,
            stream,
            uploadObjectOptions,
            cancellationToken);

        if (string.IsNullOrEmpty(_config.DefaultCacheControl)) return GetPublicUrl(objectName);

        uploadedObject.CacheControl = _config.DefaultCacheControl;
        await _storageClient.UpdateObjectAsync(uploadedObject, cancellationToken: cancellationToken);

        return GetPublicUrl(objectName);
    }

    public async Task<string> UploadAsync(byte[] data, string fileName, string contentType, string? folderPath = null, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(data);
        return await UploadAsync(stream, fileName, contentType, folderPath, cancellationToken);
    }

    public async Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(_config.BucketName, filePath, stream, cancellationToken: cancellationToken);
        return stream.ToArray();
    }

    public async Task DownloadToStreamAsync(string filePath, Stream destination, CancellationToken cancellationToken = default)
    {
        await _storageClient.DownloadObjectAsync(_config.BucketName, filePath, destination, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await _storageClient.DeleteObjectAsync(_config.BucketName, filePath, cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await _storageClient.GetObjectAsync(_config.BucketName, filePath, cancellationToken: cancellationToken);
            return true;
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public string GetPublicUrl(string filePath)
    {
        return string.IsNullOrEmpty(_config.BaseUrl) switch
        {
            false => $"{_config.BaseUrl?.TrimEnd('/')}/{filePath}",
            _ => $"https://storage.googleapis.com/{_config.BucketName}/{filePath}"
        };
    }

    public async Task<string> GetSignedUrlAsync(string filePath, int expirationMinutes = 60, CancellationToken cancellationToken = default)
    {
        var expiration = TimeSpan.FromMinutes(expirationMinutes);

        var signedUrl = await _urlSigner.SignAsync(
            _config.BucketName,
            filePath,
            expiration,
            HttpMethod.Get,
            cancellationToken: cancellationToken);

        return signedUrl;
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string? folderPath = null, CancellationToken cancellationToken = default)
    {
        var files = new List<string>();
        var prefix = string.IsNullOrEmpty(folderPath) ? null : folderPath.TrimEnd('/') + "/";

        await foreach (var obj in _storageClient.ListObjectsAsync(_config.BucketName, prefix))
        {
            files.Add(obj.Name);
        }

        return files;
    }

    public async Task CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        await _storageClient.CopyObjectAsync(
            _config.BucketName,
            sourcePath,
            _config.BucketName,
            destinationPath,
            cancellationToken: cancellationToken);
    }

    public async Task MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        await CopyAsync(sourcePath, destinationPath, cancellationToken);
        await DeleteAsync(sourcePath, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _storageClient?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
