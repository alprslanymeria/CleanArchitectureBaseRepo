using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace Base.Storage.google;

/// <summary>
/// FACTORY IMPLEMENTATION FOR CREATING GOOGLE CLOUD STORAGE CLIENTS
/// </summary>
public class GoogleCloudStorageClientFactory : IGoogleCloudStorageClientFactory
{
    // FIELDS
    private readonly GoogleCredential _credential;

    public GoogleCloudStorageClientFactory()
    {
        _credential = GoogleCredential.GetApplicationDefault();
    }


    // IMPLEMENTATION OF IGoogleCloudStorageClientFactory
    public StorageClient CreateStorageClient()
    {
        return StorageClient.Create(_credential);
    }

    public UrlSigner CreateUrlSigner()
    {
        return _credential.UnderlyingCredential switch
        {
            ServiceAccountCredential serviceAccountCredential => UrlSigner.FromCredential(serviceAccountCredential),
            _ => UrlSigner.FromCredential(_credential)
        };
    }
}
