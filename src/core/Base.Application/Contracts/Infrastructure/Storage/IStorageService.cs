using Base.Domain.Options.Storage;

namespace Base.Application.Contracts.Infrastructure.Storage;

/// <summary>
/// MAIN STORAGE SERVICE INTERFACE THAT DELEGATES TO THE CONFIGURED STORAGE PROVIDER
/// </summary>
public interface IStorageService : IStorageProvider
{
    /// <summary>
    /// GETS THE CURRENT STORAGE TYPE BEING USED
    /// </summary>
    StorageType CurrentStorageType { get; }

    /// <summary>
    /// GETS THE UNDERLYING STORAGE PROVIDER
    /// </summary>
    IStorageProvider Provider { get; }
}

/// <summary>
/// FACTORY INTERFACE FOR CREATING STORAGE PROVIDERS
/// </summary>
public interface IStorageProviderFactory
{
    /// <summary>
    /// CREATES A STORAGE PROVIDER BASED ON THE SPECIFIED TYPE
    /// </summary>
    IStorageProvider CreateProvider(StorageType storageType);

    /// <summary>
    /// GETS THE DEFAULT STORAGE PROVIDER BASED ON CONFIGURATION
    /// </summary>
    IStorageProvider GetDefaultProvider();
}