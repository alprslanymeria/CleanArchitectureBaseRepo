using Base.Application.Contracts.Infrastructure.Storage;
using Base.Domain.Options.Storage;
using Base.Storage.aws;
using Base.Storage.google;
using Base.Storage.local;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Base.Storage;

public static class StorageExtension
{
    public static IServiceCollection AddStorageServicesExt(this IServiceCollection services, IConfiguration configuration)
    {
        // LOAD STORAGE CONFIGURATION AND VALIDATE
        var storageConfig = configuration
            .GetRequiredSection(StorageConfig.Key)
            .Get<StorageConfig>()
            ?? throw new InvalidOperationException("StorageConfig is missing in appsettings");

        // CONFIGURATION BINDINGS
        services.Configure<StorageConfig>(configuration.GetSection(StorageConfig.Key));
        services.Configure<LocalStorageConfig>(configuration.GetSection(LocalStorageConfig.Key));
        services.Configure<GoogleCloudStorageConfig>(configuration.GetSection(GoogleCloudStorageConfig.Key));
        services.Configure<AwsS3StorageConfig>(configuration.GetSection(AwsS3StorageConfig.Key));

        // COMMON STORAGE SERVICES
        services.AddScoped<IStorageService, StorageService>();

        // REGISTRATION BASED ON STORAGE TYPE
        switch (storageConfig.StorageType)
        {
            case StorageType.GoogleCloud:
                AddGoogleCloudStorage(services);
                break;

            case StorageType.AwsS3:
                AddAwsS3Storage(services);
                break;

            case StorageType.Local:
                AddLocalStorage(services);
                break;

            default:
                throw new NotSupportedException($"Storage type '{storageConfig.StorageType}' is not supported.");
        }

        return services;
    }

    private static void AddLocalStorage(IServiceCollection services)
    {
        services.AddSingleton<ILocalFileSystemFactory, LocalFileSystemFactory>();
        services.AddScoped<IStorageProvider, LocalFileStorageProvider>();
    }

    private static void AddGoogleCloudStorage(IServiceCollection services)
    {
        services.AddSingleton<IGoogleCloudStorageClientFactory, GoogleCloudStorageClientFactory>();
        services.AddScoped<IStorageProvider, GoogleCloudStorageProvider>();
    }

    private static void AddAwsS3Storage(IServiceCollection services)
    {
        services.AddSingleton<IAwsS3ClientFactory, AwsS3ClientFactory>();
        services.AddScoped<IStorageProvider, AwsS3StorageProvider>();
    }
}
