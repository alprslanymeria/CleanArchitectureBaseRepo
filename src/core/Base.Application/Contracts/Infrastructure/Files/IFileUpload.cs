namespace Base.Application.Contracts.Infrastructure.Files;

/// <summary>
/// ABSTRACTION FOR FILE UPLOAD OPERATIONS.
/// THIS INTERFACE ISOLATES THE APPLICATION LAYER FROM WEB FRAMEWORK-SPECIFIC FILE UPLOAD MECHANISMS.
/// </summary>
public interface IFileUpload
{
    /// <summary>
    /// GETS THE FILE NAME INCLUDING EXTENSION.
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// GETS THE CONTENT TYPE (MIME TYPE) OF THE FILE.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// GETS THE LENGTH OF THE FILE IN BYTES.
    /// </summary>
    long Length { get; }

    /// <summary>
    /// OPENS THE FILE STREAM FOR READING.
    /// </summary>
    Stream OpenReadStream();
}
