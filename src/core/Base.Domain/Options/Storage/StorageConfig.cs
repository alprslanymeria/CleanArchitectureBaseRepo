namespace Base.Domain.Options.Storage;


/// <summary>
/// MAIN STORAGE CONFIGURATION THAT DETERMINES WHICH PROVIDER TO USE
/// </summary>
public class StorageConfig
{
    public const string Key = "StorageConfig";

    /// <summary>
    /// THE TYPE OF STORAGE PROVIDER TO USE
    /// </summary>
    public StorageType StorageType { get; set; } = StorageType.GoogleCloud;

    /// <summary>
    /// WHETHER TO ENABLE STORAGE SERVICE
    /// </summary>
    public bool Enabled { get; set; } = true;
}