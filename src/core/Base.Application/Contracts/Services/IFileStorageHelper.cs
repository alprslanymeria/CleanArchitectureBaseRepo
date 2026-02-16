namespace Base.Application.Contracts.Services;

/// <summary>
/// HELPER SERVICE FOR COMMON FILE STORAGE OPERATIONS.
/// THIS PROVIDES CENTRALIZED FILE UPLOAD/DELETE LOGIC ACROSS HANDLERS.
/// </summary>
public interface IFileStorageHelper
{
    /// <summary>
    /// UPLOADS A FILE TO STORAGE WITH A STANDARDIZED PATH STRUCTURE.
    /// </summary>
    /// <param name="file">THE FILE TO UPLOAD</param>
    /// <param name="userId">THE USER ID FOR PATH ORGANIZATION</param>
    /// <param name="folderName">THE FOLDER NAME FOR CATEGORIZATION (E.G., "RBOOKS", "WBOOKS")</param>
    /// <returns>THE URL OF THE UPLOADED FILE</returns>
    Task<string> UploadFileToStorageAsync(IFileUpload file, string userId, string folderName);

    /// <summary>
    /// DELETES A FILE FROM STORAGE IF IT EXISTS.
    /// </summary>
    /// <param name="fileUrl">THE URL/PATH OF THE FILE TO DELETE</param>
    /// <returns>TASK REPRESENTING THE ASYNC OPERATION</returns>
    Task DeleteFileFromStorageAsync(string fileUrl);
}
