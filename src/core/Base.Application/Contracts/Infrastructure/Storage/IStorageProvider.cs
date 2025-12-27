namespace Base.Application.Contracts.Infrastructure.Storage;

/// <summary>
/// BASE INTERFACE FOR ALL STORAGE PROVIDERS (Local, CLOUD, ETC.)
/// </summary>
public interface IStorageProvider : IDisposable
{
    /// <summary>
    /// GETS THE PROVIDER NAME FOR IDENTIFICATION
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// UPLOADS A FILE TO STORAGE
    /// </summary>
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folderPath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// UPLOADS A FILE FROM BYTE ARRAY TO STORAGE
    /// </summary>
    Task<string> UploadAsync(byte[] data, string fileName, string contentType, string? folderPath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// DOWNLOADS A FILE FROM STORAGE AS BYTE ARRAY
    /// </summary>
    Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// DOWNLOADS A FILE FROM STORAGE TO A STREAM
    /// </summary>
    Task DownloadToStreamAsync(string filePath, Stream destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// DELETES A FILE FROM STORAGE
    /// </summary>
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// CHECK IF A FILE EXISTS IN STORAGE
    /// </summary>
    Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// GETS THE PUBLIC/ACCESSIBLE URL FOR A FILE
    /// </summary>
    string GetPublicUrl(string filePath);

    /// <summary>
    /// GENERATES A SIGNED / TEMPORARY URL FOR ACCESSING A FILE (IF SUPPORTED)
    /// </summary>
    Task<string> GetSignedUrlAsync(string filePath, int expirationMinutes = 60, CancellationToken cancellationToken = default);

    /// <summary>
    /// LISTS ALL FILES IN A FOLDER
    /// </summary>
    Task<IEnumerable<string>> ListFilesAsync(string? folderPath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// COPIES A FILE WITHIN STORAGE
    /// </summary>
    Task CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// MOVES A FILE WITHIN STORAGE
    /// </summary>
    Task MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
}
