namespace Base.Domain.Options.Storage;

/// <summary>
/// CONFIGURATION FOR LOCAL FILE SYSTEM STORAGE
/// </summary>
public class LocalStorageConfig
{
    public const string Key = "LocalStorage";

    /// <summary>
    /// ROOT DIRECTORY PATH FOR FILE STORAGE CAN BE ABSOLUTE OR RELATIVE TO THE APPLICATION
    /// </summary>
    public string RootPath { get; set; } = "uploads";

    /// <summary>
    /// BASE URL FOR SERVING FILES (E.G., HTTPS://YOURDOMAIN.COM/FILES) IF EMPTY, FILES WILL BE SERVED FROM THE APPLICATION
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// WHETHER TO CREATE DIRECTORIES IF THEY DON'T EXIST
    /// </summary>
    public bool CreateDirectoriesIfNotExist { get; set; } = true;

    /// <summary>
    /// MAXIMUM FILE SIZE IN BYTES (0 = UNLIMITED)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 0;

    /// <summary>
    /// ALLOWED FILE EXTENSIONS (EMPTY = ALL ALLOWED)   
    /// </summary>
    public string[] AllowedExtensions { get; set; } = [];
}