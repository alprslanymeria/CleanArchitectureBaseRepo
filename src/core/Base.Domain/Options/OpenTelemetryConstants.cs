namespace Base.Domain.Options;

public class OpenTelemetryConstants
{
    public const string Key = "OpenTelemetry";

    /// <summary>
    /// THE NAME OF THE APPLICATION/SERVICE THAT COLLECTS TELEMETRY DATA
    /// </summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>
    /// VERSION OF THE SERVICE
    /// </summary>
    public string ServiceVersion { get; set; } = null!;

    /// <summary>
    /// TRACE SOURCE IN APPLICATION/SERVICE
    /// </summary>
    public string ActivitySourceName { get; set; } = null!;
}