using Amazon.S3;

namespace Base.Storage.aws;

/// <summary>
/// FACTORY INTERFACE FOR CREATING AWS S3 CLIENTS
/// </summary>
public interface IAwsS3ClientFactory
{
    /// <summary>
    /// CREATES AN AMAZON S3 CLIENT INSTANCE
    /// </summary>
    IAmazonS3 CreateS3Client();
}