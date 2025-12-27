using Amazon;
using Amazon.S3;
using Base.Domain.Options.Storage;
using Microsoft.Extensions.Options;

namespace Base.Storage.aws;

/// <summary>
/// FACTORY IMPLEMENTATION FOR CREATING AWS S3 CLIENTS
/// </summary>
public class AwsS3ClientFactory(IOptions<AwsS3StorageConfig> config) : IAwsS3ClientFactory
{
    // FIELDS
    private readonly AwsS3StorageConfig _config = config.Value;

    // IMPLEMENTATION OF IAwsS3ClientFactory
    public IAmazonS3 CreateS3Client()
    {
        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_config.Region),
            ForcePathStyle = _config.UsePathStyleUrls
        };

        if (!string.IsNullOrEmpty(_config.ServiceUrl))
        {
            s3Config.ServiceURL = _config.ServiceUrl;
        }

        return new AmazonS3Client(_config.AccessKeyId, _config.SecretAccessKey, s3Config);
    }
}