using Base.Application.Contracts.Infrastructure.Storage;
using Base.Storage.aws;
using Base.Storage.google;
using Base.Storage.local;
using Microsoft.Extensions.DependencyInjection;

namespace Base.Storage;
public static class StorageExtension
{
    public static IServiceCollection AddStorageServicesExt(this IServiceCollection services)
    {

        // STORAGE PROVIDER CLIENT FACTORIES
        services.AddSingleton<IGoogleCloudStorageClientFactory, GoogleCloudStorageClientFactory>();
        services.AddSingleton<IAwsS3ClientFactory, AwsS3ClientFactory>();
        services.AddSingleton<ILocalFileSystemFactory, LocalFileSystemFactory>();

        // STORAGE PROVIDERS (REGISTERED FOR FACTORY RESOLUTION)
        services.AddScoped<LocalFileStorageProvider>();
        services.AddScoped<GoogleCloudStorageProvider>();
        services.AddScoped<AwsS3StorageProvider>();

        // STORAGE FACTORY AND SERVICE
        services.AddScoped<IStorageProviderFactory, StorageProviderFactory>();
        services.AddScoped<IStorageService, StorageService>();

        return services;
    }
}