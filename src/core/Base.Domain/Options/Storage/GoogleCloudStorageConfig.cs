namespace Base.Domain.Options.Storage;

/// <summary>
/// CONFIGURATION FOR GOOGLE CLOUD STORAGE
/// </summary>
public class GoogleCloudStorageConfig
{
    public const string Key = "GoogleCloudStorage";

    /// <summary>
    /// GOOGLE CLOUD STORAGE BUCKET NAME
    /// </summary>
    public string BucketName { get; set; } = string.Empty;
}