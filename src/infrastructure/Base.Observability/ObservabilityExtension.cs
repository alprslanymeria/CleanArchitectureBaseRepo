using Base.Domain.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Base.Observability;

public static class ObservabilityExtension
{
    public static IServiceCollection AddOpenTelemetryServicesExt(this IServiceCollection services, IConfiguration configuration)
    {
        // GET OTEL CONSTANTS FROM APP SETTINGS
        var openTelemetryConstants = configuration.GetRequiredSection(OpenTelemetryConfig.Key).Get<OpenTelemetryConfig>();

        // CHECK IF OTEL IS ENABLED
        if (openTelemetryConstants is null || !openTelemetryConstants.Enabled)
        {
            return services;
        }

        // SET ACTIVITY SOURCE
        ActivitySourceProvider.Source = new System.Diagnostics.ActivitySource(openTelemetryConstants.ActivitySourceName);

        services.AddOpenTelemetry()
            .WithTracing(configure =>
            {

                configure
                    .AddSource(openTelemetryConstants.ActivitySourceName)
                    .ConfigureResource(resource =>
                    {
                        resource.AddService(openTelemetryConstants.ServiceName, serviceVersion: openTelemetryConstants.ServiceVersion);
                    });

                // ENRICH METHODS ARE HOOK POINTS THAT ALLOW TO ADD CUSTOM TAG'S TO CREATED BY OpenTelemetry DATA'S

                // ASPNET CORE INSTRUMENTATION
                configure.AddAspNetCoreInstrumentation(aspNetCoreOptions =>
                {

                    //WE CAN TAKE EXCEPTION WITH TWO WAYS: TRACE OR LOGS
                    // IF WE WANT TAKE WITH TRACE WE MUST SET THIS PROPERTY TRUE
                    // IF WE WANT TAKE WITH LOGS WE MUST SET THIS PROPERTY FALSE AND CONFIGURE LOGS INSTEAD
                    // FOR SEE EXCEPTION DETAILS IN TRACE TAG'S "otel.status_code" & "otel.status_description"
                    aspNetCoreOptions.RecordException = true;

                    // FILTER TO COLLECT ONLY API REQUESTS
                    aspNetCoreOptions.Filter = (context) => !string.IsNullOrEmpty(context.Request.Path.Value) && context.Request.Path.Value.Contains("api", StringComparison.InvariantCulture);

                    // WE CAN TAKE EXCEPTION WITH TWO WAYS: TRACE OR LOGS
                    // IF WE WANT TAKE WITH TRACE WE MUST SET THIS PROPERTY TRUE
                    // IF WE WANT TAKE WITH LOGS WE MUST SET THIS PROPERTY FALSE AND CONFIGURE LOGS INSTEAD
                    // FOR SEE EXCEPTION DETAILS IN TRACE TAG'S "otel.status_code" & "otel.status_description"
                    aspNetCoreOptions.RecordException = true;
                });

                // ENTITY FRAMEWORK CORE INSTRUMENTATION
                configure.AddEntityFrameworkCoreInstrumentation(efcoreOptions =>
                {

                });


                // HTTP INSTRUMENTATION
                configure.AddHttpClientInstrumentation(httpClientOptions =>
                {
                    // FOR SEE EXCEPTION DETAILS IN TRACE TAG'S "otel.status_code" & "otel.status_description"
                    httpClientOptions.RecordException = true;


                    httpClientOptions.FilterHttpRequestMessage = (request) => request.RequestUri!.AbsoluteUri.Contains("9200", StringComparison.InvariantCulture);

                    // ADD REQUEST BODY AS TAG TO ACTIVITY
                    httpClientOptions.EnrichWithHttpRequestMessage = async (activity, request) =>
                    {
                        var requestContent = "empty";

                        if (request.Content != null)
                        {
                            requestContent = await request.Content.ReadAsStringAsync();
                        }

                        activity.SetTag("http.request.body", requestContent);
                    };

                    httpClientOptions.EnrichWithHttpResponseMessage = async (activity, response) =>
                    {

                        if (response.Content != null)
                        {
                            activity.SetTag("http.response.body", await response.Content.ReadAsStringAsync());
                        }
                    };
                });


                // REDIS INSTRUMENTATION
                configure.AddRedisInstrumentation(redisOptions =>
                {
                    redisOptions.SetVerboseDatabaseStatements = true;
                });


                // TRACE EXPORTERS FROM APPSETTINGS
                ConfigureTraceExporters(configure, openTelemetryConstants);

            })
            .WithMetrics(options =>
            {
                options.AddMeter("metric.meter.api");
                options.ConfigureResource(resource =>
                {
                    resource.AddService(openTelemetryConstants.ServiceName, serviceVersion: openTelemetryConstants.ServiceVersion);
                });

                // BUILT-IN METRIC INSTRUMENTATION
                options.AddAspNetCoreInstrumentation();
                options.AddHttpClientInstrumentation();
                options.AddRuntimeInstrumentation();

                // METRICS EXPORTERS FROM APPSETTINGS
                ConfigureMetricsExporters(options, openTelemetryConstants);
            });


        return services;
    }

    public static void AddOpenTelemetryLogExt(this WebApplicationBuilder builder)
    {
        // GET OTEL CONSTANTS FROM APP SETTINGS
        var openTelemetryConstants = builder.Configuration.GetRequiredSection(OpenTelemetryConfig.Key).Get<OpenTelemetryConfig>();

        // CHECK IF OTEL IS ENABLED
        if (openTelemetryConstants is null || !openTelemetryConstants.Enabled)
        {
            return;
        }

        builder.Logging.AddOpenTelemetry(options =>
        {
            // FILL RESOURCE BUILDER WITH SERVICE NAME AND VERSION
            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(openTelemetryConstants.ServiceName ?? "UnknownService", serviceVersion: openTelemetryConstants.ServiceVersion);

            // SET RESOURCE BUILDER TO CONFIGURATION
            options.SetResourceBuilder(resourceBuilder);

            // LOG EXPORTERS FROM APPSETTINGS
            ConfigureLogExporters(options, openTelemetryConstants);
        });
    }

    private static void ConfigureLogExporters(OpenTelemetryLoggerOptions options, OpenTelemetryConfig constants)
    {
        if (constants.LogExporters is null || constants.LogExporters.Length == 0)
        {
            return;
        }

        foreach (var exporter in constants.LogExporters)
        {
            switch (exporter.ToLowerInvariant())
            {
                case "console":
                    options.AddConsoleExporter();
                    break;

                case "elasticsearch":
                    options.AddOtlpExporter();
                    break;
            }
        }
    }

    private static void ConfigureTraceExporters(TracerProviderBuilder builder, OpenTelemetryConfig constants)
    {
        if (constants.TraceExporters is null || constants.TraceExporters.Length == 0)
        {
            return;
        }

        foreach (var exporter in constants.TraceExporters)
        {
            switch (exporter.ToLowerInvariant())
            {
                case "console":
                    builder.AddConsoleExporter();
                    break;

                case "jaeger":
                    builder.AddOtlpExporter();
                    break;
            }
        }
    }

    private static void ConfigureMetricsExporters(MeterProviderBuilder builder, OpenTelemetryConfig constants)
    {
        if (constants.MetricsExporters is null || constants.MetricsExporters.Length == 0)
        {
            return;
        }

        foreach (var exporter in constants.MetricsExporters)
        {
            switch (exporter.ToLowerInvariant())
            {
                case "console":
                    builder.AddConsoleExporter();
                    break;

                case "prometheus":
                    builder.AddOtlpExporter();
                    break;
            }
        }
    }
}
