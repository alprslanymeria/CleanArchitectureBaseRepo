using Base.Application.Contracts.Infrastructure.Storage;
using Microsoft.Extensions.Logging;

namespace Base.Integration;

/// <summary>
/// HELPER SERVICE FOR COMMON FILE STORAGE OPERATIONS.
/// </summary>
public class FileStorageHelper(

    IStorageService storageService,
    ILogger<FileStorageHelper> logger

    ) : IFileStorageHelper
{
    public async Task<string> UploadFileToStorageAsync(IFileUpload file, string userId, string folderName)
    {
        var fileName = $"{userId}/{folderName}/{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{file.FileName}";

        using var stream = file.OpenReadStream();
        var fileUrl = await storageService.UploadAsync(stream, fileName, file.ContentType, null);

        logger.LogInformation("FileStorageHelper -> SUCCESSFULLY UPLOADED FILE TO STORAGE: {FileUrl}", fileUrl);

        return fileUrl;
    }

    public async Task DeleteFileFromStorageAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return;
        }

        try
        {

            // EXTRACT FILE PATH FROM URL
            // Remove domain and bucket name to get relative path
            // https://storage.googleapis.com/digital-receipt-app/user/folder/file -> user/folder/file

            var filePath = fileUrl.Replace("https://storage.googleapis.com/digital-receipt-app/", "");

            var fileExists = await storageService.ExistsAsync(filePath);

            if (!fileExists)
            {
                return;
            }

            await storageService.DeleteAsync(filePath);
            logger.LogInformation("FileStorageHelper -> SUCCESSFULLY DELETED FILE FROM STORAGE: {FileUrl}", filePath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "FileStorageHelper -> FAILED TO DELETE FILE FROM STORAGE: {FileUrl}", fileUrl);
        }
    }
}
