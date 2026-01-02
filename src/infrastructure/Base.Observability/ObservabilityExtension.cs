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
        // GET OpenTelemetry CONSTANTS FROM APP SETTINGS
        var openTelemetryConstants = configuration.GetSection(OpenTelemetryConstants.Key).Get<OpenTelemetryConstants>();

        // SET ACTIVITY SOURCE
        ActivitySourceProvider.Source = new System.Diagnostics.ActivitySource(openTelemetryConstants!.ActivitySourceName);

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


                // TRACE EXPORTERS
                configure.AddConsoleExporter();
                configure.AddOtlpExporter();

            }).WithMetrics(options =>
            {
                options.AddMeter("metric.meter.api");
                options.ConfigureResource(resource =>
                {
                    resource.AddService("Metric.API", serviceVersion: "1.0.0");
                });

                // METRICS EXPORTERS
                options.AddConsoleExporter();
                options.AddOtlpExporter();
            });


        return services;
    }

    public static void AddOpenTelemetryLogExt(this WebApplicationBuilder builder)
    {
        // GET OpenTelemetry SETTINGS FROM CONFIGURATION
        var config = builder.Configuration.GetSection("OpenTelemetry");
        var serviceName = config.GetValue<string>("ServiceName");
        var serviceVersion = config.GetValue<string>("ServiceVersion");

        builder.Logging.AddOpenTelemetry(options =>
        {
            // FILL RESOURCE BUILDER WITH SERVICE NAME AND VERSION
            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(serviceName ?? "UnknownService", serviceVersion: serviceVersion);

            // SET RESOURCE BUILDER TO CONFIGURATION
            options.SetResourceBuilder(resourceBuilder);

            // ADD OTLP EXPORTER TO SEND LOGS TO OTEL COLLECTOR
            options.AddOtlpExporter();
        });
    }
}