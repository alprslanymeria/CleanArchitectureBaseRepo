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
