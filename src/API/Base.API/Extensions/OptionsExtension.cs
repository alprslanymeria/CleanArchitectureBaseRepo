using Base.Domain.Options.Caching;
using Base.Domain.Options.Storage;

namespace Base.API.Extensions;

public static class OptionsExtension
{
    public static IServiceCollection AddOptionsPattern(this IServiceCollection services, IConfiguration configuration)
    {
        // OPTIONS PATTERN
        services.Configure<DistributedCacheConfig>(configuration.GetSection(nameof(DistributedCacheConfig)));
        services.Configure<CacheConfig>(configuration.GetSection(nameof(CacheConfig)));
        services.Configure<StorageConfig>(configuration.GetSection(StorageConfig.Key));
        services.Configure<LocalStorageConfig>(configuration.GetSection(nameof(LocalStorageConfig)));
        services.Configure<GoogleCloudStorageConfig>(configuration.GetSection(nameof(GoogleCloudStorageConfig)));
        services.Configure<AwsS3StorageConfig>(configuration.GetSection(nameof(AwsS3StorageConfig)));

        return services;
    }
}
