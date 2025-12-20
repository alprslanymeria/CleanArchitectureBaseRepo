using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace Base.Infrastructure.Observability;

public static class OpenTelemetryLoggingConfigurator
{
    public static void ConfigureLogging(OpenTelemetryLoggerOptions options, string serviceName, string? serviceVersion)
    {
        // FILL RESOURCE BUILDER WITH SERVICE NAME AND VERSION
        var resourceBuilder = ResourceBuilder
                                .CreateDefault()
                                .AddService(serviceName ?? "UnknownService", serviceVersion: serviceVersion);

        // SET RESOURCE BUILDER TO CONFIGURATION
        options.SetResourceBuilder(resourceBuilder);

        // ADD OTLP EXPORTER TO SEND LOGS TO OTEL COLLECTOR
        options.AddOtlpExporter();
    }
}
