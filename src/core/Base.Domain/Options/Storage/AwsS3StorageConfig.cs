namespace App.Domain.Options.Storage;

/// <summary>
/// CONFIGURATION FOR AWS S3 STORAGE
/// </summary>
public class AwsS3StorageConfig
{
    public const string Key = "AwsS3Storage";

    /// <summary>
    /// AWS ACCESS KEY ID
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// AWS SECRET ACCESS KEY
    /// </summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS REGION (E.G., US-EAST-1, EU-WEST-1)
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// S3 BUCKET NAME
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// CUSTOM SERVICE URL (FOR S3-COMPATIBLE SERVICES LIKE MINIO)
    /// </summary>
    public string? ServiceUrl { get; set; }

    /// <summary>
    /// BASE URL FOR PUBLIC ACCESS (OPTIONAL, USES S3 URL IF EMPTY)
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// DEFAULT CACHE CONTROL HEADER FOR UPLOADED FILES
    /// </summary>
    public string? DefaultCacheControl { get; set; } = "public, max-age=31536000";

    /// <summary>
    /// WHETHER TO USE PATH-STYLE URLS (FOR S3-COMPATIBLE SERVICES)
    /// </summary>
    public bool UsePathStyleUrls { get; set; } = false;
}
