namespace Base.Domain.Options.Storage;

/// <summary>
/// Defines the available storage provider types
/// </summary>
public enum StorageType
{
    /// <summary>
    /// LOCAL FILE SYSTEM STORAGE
    /// </summary>
    Local,

    /// <summary>
    /// GOOGLE CLOUD STORAGE
    /// </summary>
    GoogleCloud,

    /// <summary>
    /// AMAZON WEB SERVICES S3
    /// </summary>
    AwsS3,

    /// <summary>
    /// AZURE BLOB STORAGE
    /// </summary>
    AzureBlob
}