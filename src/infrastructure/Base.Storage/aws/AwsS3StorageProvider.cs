using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Base.Application.Contracts.Infrastructure.Storage;
using Base.Domain.Options.Storage;
using Microsoft.Extensions.Options;

namespace Base.Storage.aws;

/// <summary>
/// AWS S3 STORAGE PROVIDER IMPLEMENTATION
/// </summary>
public class AwsS3StorageProvider(

    IAwsS3ClientFactory clientFactory,
    IOptions<AwsS3StorageConfig> config

    ) : IStorageProvider
{
    // FIELDS
    private readonly IAmazonS3 _s3Client = clientFactory.CreateS3Client();
    private readonly AwsS3StorageConfig _config = config.Value;
    private bool _disposed;

    #region UTILS
    /// <summary>
    /// CREATE FILE NAME AND PATH
    /// </summary>
    private static string BuildObjectKey(string fileName, string? folderPath)
        => string.IsNullOrEmpty(folderPath) ? fileName : $"{folderPath.Trim('/')}/{fileName}";
    #endregion

    // IMPLEMENTATION OF IStorageProvider
    public string ProviderName => "AwsS3Storage";

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folderPath = null, CancellationToken cancellationToken = default)
    {
        var objectKey = BuildObjectKey(fileName, folderPath);

        var request = new PutObjectRequest
        {
            BucketName = _config.BucketName,
            Key = objectKey,
            InputStream = stream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };

        if (!string.IsNullOrEmpty(_config.DefaultCacheControl))
        {
            request.Headers.CacheControl = _config.DefaultCacheControl;
        }

        await _s3Client.PutObjectAsync(request, cancellationToken);

        return GetPublicUrl(objectKey);
    }

    public async Task<string> UploadAsync(byte[] data, string fileName, string contentType, string? folderPath = null, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(data);
        return await UploadAsync(stream, fileName, contentType, folderPath, cancellationToken);
    }

    public async Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = _config.BucketName,
            Key = filePath
        };

        using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    public async Task DownloadToStreamAsync(string filePath, Stream destination, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = _config.BucketName,
            Key = filePath
        };

        using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        await response.ResponseStream.CopyToAsync(destination, cancellationToken);
    }

    public async Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _config.BucketName,
            Key = filePath
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _config.BucketName,
                Key = filePath
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public string GetPublicUrl(string filePath)
    {
        if (!string.IsNullOrEmpty(_config.BaseUrl))
        {
            return $"{_config.BaseUrl.TrimEnd('/')}/{filePath}";
        }

        if (!string.IsNullOrEmpty(_config.ServiceUrl))
        {
            return $"{_config.ServiceUrl.TrimEnd('/')}/{_config.BucketName}/{filePath}";
        }

        return $"https://{_config.BucketName}.s3.{_config.Region}.amazonaws.com/{filePath}";
    }

    public Task<string> GetSignedUrlAsync(string filePath, int expirationMinutes = 60, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _config.BucketName,
            Key = filePath,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Verb = HttpVerb.GET
        };

        var signedUrl = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(signedUrl);
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string? folderPath = null, CancellationToken cancellationToken = default)
    {
        var files = new List<string>();
        var prefix = string.IsNullOrEmpty(folderPath) ? null : folderPath.TrimEnd('/') + "/";

        var request = new ListObjectsV2Request
        {
            BucketName = _config.BucketName,
            Prefix = prefix
        };

        ListObjectsV2Response response;
        do
        {
            response = await _s3Client.ListObjectsV2Async(request, cancellationToken);

            foreach (var obj in response.S3Objects)
            {
                files.Add(obj.Key);
            }

            request.ContinuationToken = response.NextContinuationToken;

        } while (response.IsTruncated == true);

        return files;
    }

    public async Task CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        var request = new CopyObjectRequest
        {
            SourceBucket = _config.BucketName,
            SourceKey = sourcePath,
            DestinationBucket = _config.BucketName,
            DestinationKey = destinationPath
        };

        await _s3Client.CopyObjectAsync(request, cancellationToken);
    }

    public async Task MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        await CopyAsync(sourcePath, destinationPath, cancellationToken);
        await DeleteAsync(sourcePath, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _s3Client?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
