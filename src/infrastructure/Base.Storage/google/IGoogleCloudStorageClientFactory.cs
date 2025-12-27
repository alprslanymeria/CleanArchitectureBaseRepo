using Google.Cloud.Storage.V1;

namespace Base.Storage.google;

/// <summary>
/// FACTORY INTERFACE FOR CREATING GOOGLE CLOUD STORAGE CLIENTS
/// </summary>
public interface IGoogleCloudStorageClientFactory
{
    /// <summary>
    /// CREATES A STORAGE CLIENT INSTANCE
    /// </summary>
    StorageClient CreateStorageClient();

    /// <summary>
    /// CREATES A URL SIGNER INSTANCE FOR GENERATING SIGNED URLS
    /// </summary>
    UrlSigner CreateUrlSigner();
}