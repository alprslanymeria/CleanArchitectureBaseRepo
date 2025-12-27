using Base.Observability;

namespace Base.API.Extensions;

public static class OpenTelemetryLoggingExtension
{
    public static void AddOpenTelemetryLog(this WebApplicationBuilder builder)
    {
        // GET OpenTelemetry SETTINGS FROM CONFIGURATION
        var config = builder.Configuration.GetSection("OpenTelemetry");
        var serviceName = config.GetValue<string>("ServiceName");
        var serviceVersion = config.GetValue<string>("ServiceVersion");

        builder.Logging.AddOpenTelemetry(options =>
        {
            OpenTelemetryLoggingConfigurator.ConfigureLogging(
                options,
                serviceName!,
                serviceVersion);
        });
    }
}
