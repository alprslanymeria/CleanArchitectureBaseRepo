namespace Base.Domain.Options;

public class OpenTelemetryConstants
{
    public const string Key = "OpenTelemetry";
    public string ServiceName { get; set; } = default!;
    public string ServiceVersion { get; set; } = default!;
    public string ActivitySourceName { get; set; } = default!;
}
