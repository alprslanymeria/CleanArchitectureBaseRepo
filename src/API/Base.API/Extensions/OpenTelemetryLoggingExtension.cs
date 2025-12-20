using Base.Infrastructure.Observability;

namespace Base.API.Extensions;

public static class OpenTelemetryLoggingExtension
{
    public static void AddOpenTelemetryLog(this WebApplicationBuilder builder)
    {
        // GET OPENTELEMETRY SETTINGS FROM CONFIGURATION
        var otelSection = builder.Configuration.GetSection("OpenTelemetry");
        var serviceName = otelSection.GetValue<string>("ServiceName");
        var serviceVersion = otelSection.GetValue<string>("ServiceVersion");

        builder.Logging.AddOpenTelemetry(options =>
        {
            OpenTelemetryLoggingConfigurator.ConfigureLogging(
                options,
                serviceName!,
                serviceVersion);
        });
    }
}
