using Base.Domain.Options;
using Base.Observability;

namespace Base.API.Extensions;

public static class OpenTelemetryExtension
{
    public static IServiceCollection AddOpenTelemetryExtension(this IServiceCollection services, IConfiguration configuration)
    {
        // GET OpenTelemetry CONSTANTS FROM APP SETTINGS
        var openTelemetryConstants = configuration.GetSection(OpenTelemetryConstants.Key).Get<OpenTelemetryConstants>();

        // SET ACTIVITY SOURCE
        ActivitySourceProvider.Source = new System.Diagnostics.ActivitySource(openTelemetryConstants!.ActivitySourceName);

        services.AddOpenTelemetry()
            .WithTracing(configure =>
            {
                OpenTelemetryConfigurator.ConfigureTracing(configure, openTelemetryConstants);

            }).WithMetrics(options =>
            {
                OpenTelemetryConfigurator.ConfigureMetrics(options, openTelemetryConstants);
            });


        return services;
    }
}
