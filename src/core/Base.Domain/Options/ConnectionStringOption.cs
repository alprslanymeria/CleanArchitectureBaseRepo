namespace Base.Domain.Options;

public class ConnectionStringOption
{
    public const string Key = "ConnectionStrings";

    /// <summary>
    /// SQL SERVER CONN STRING
    /// </summary>
    public string SqlServer { get; set; } = null!;
}