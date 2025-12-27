namespace Base.Domain.Options.Storage;

/// <summary>
/// CONFIGURATION FOR GOOGLE CLOUD STORAGE
/// </summary>
public class GoogleCloudStorageConfig
{
    public const string Key = "GoogleCloudStorage";

    /// <summary>
    /// PATH TO THE SERVICE ACCOUNT JSON CREDENTIAL FILE
    /// </summary>
    public string CredentialFilePath { get; set; } = string.Empty;

    /// <summary>
    /// GOOGLE CLOUD STORAGE BUCKET NAME
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// BASE URL FOR PUBLIC ACCESS (OPTIONAL)
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// DEFAULT CACHE CONTROL HEADER FOR UPLOADED FILES
    /// </summary>
    public string? DefaultCacheControl { get; set; } = "public, max-age=31536000";
}