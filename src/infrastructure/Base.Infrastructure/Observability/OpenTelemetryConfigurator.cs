using Base.Domain.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Base.Infrastructure.Observability;

public static class OpenTelemetryConfigurator
{
    public static void ConfigureTracing(TracerProviderBuilder builder, OpenTelemetryConstants constants)
    {
        // SET ACTIVITY SOURCE
        ActivitySourceProvider.Source = new System.Diagnostics.ActivitySource(constants.ActivitySourceName);

        builder
            .AddSource(constants.ActivitySourceName)
            .ConfigureResource(resource =>
            {
                resource.AddService(constants.ServiceName, serviceVersion: constants.ServiceVersion);
            });

        // ENRICH METHOD'S ARE HOOK POINTS THAT ALLOW TO ADD CUSTOM TAG'S TO CREATED BY OPENTELEMETRY DATA'S

        // ASPNET CORE INSTRUMENTATION
        builder.AddAspNetCoreInstrumentation(aspNetCoreOptions =>
        {

            //WE CAN TAKE EXCEPTION WITH TWO WAYS: TRACE OR LOGS
            // IF WE WANT TAKE WITH TRACE WE MUST SET THIS PROPERTY TRUE
            // IF WE WANT TAKE WITH LOGS WE MUST SET THIS PROPERTY FALSE AND CONFIGURE LOGS INSTEAD
            // FOR SEE EXCEPTION DETAILS IN TRACE TAG'S "otel.status_code" & "otel.status_description"
            aspNetCoreOptions.RecordException = true;

            // FILTER TO COLLECT ONLY API REQUESTS
            aspNetCoreOptions.Filter = (context) =>
            {

                if (!string.IsNullOrEmpty(context.Request.Path.Value))
                {
                    return context.Request.Path.Value.Contains("api", StringComparison.InvariantCulture);
                }
                return false;

            };

            // WE CAN TAKE EXCEPTION WITH TWO WAYS: TRACE OR LOGS
            // IF WE WANT TAKE WITH TRACE WE MUST SET THIS PROPERTY TRUE
            // IF WE WANT TAKE WITH LOGS WE MUST SET THIS PROPERTY FALSE AND CONFIGURE LOGS INSTEAD
            // FOR SEE EXCEPTION DETAILS IN TRACE TAG'S "otel.status_code" & "otel.status_description"
            aspNetCoreOptions.RecordException = true;
        });

        // ENTITY FRAMEWORK CORE INSTRUMENTATION
        builder.AddEntityFrameworkCoreInstrumentation(efcoreOptions =>
        {

        });


        // HTTP INSTRUMENTATION
        builder.AddHttpClientInstrumentation(httpClientOptions =>
        {
            // FOR SEE EXCEPTION DETAILS IN TRACE TAG'S "otel.status_code" & "otel.status_description"
            httpClientOptions.RecordException = true;


            httpClientOptions.FilterHttpRequestMessage = (request) =>
            {
                return request.RequestUri!.AbsoluteUri.Contains("9200", StringComparison.InvariantCulture);
            };

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
        builder.AddRedisInstrumentation(redisOptions =>
        {
            redisOptions.SetVerboseDatabaseStatements = true;
        });


        // TRACE EXPORTERS
        builder.AddConsoleExporter();
        builder.AddOtlpExporter();
    }

    public static void ConfigureMetrics(MeterProviderBuilder builder, OpenTelemetryConstants constants)
    {
        builder.AddMeter("metric.meter.api");
        builder.ConfigureResource(resource =>
        {
            resource.AddService("Metric.API", serviceVersion: "1.0.0");
        });

        // METRICS EXPORTERS
        builder.AddConsoleExporter();
        builder.AddOtlpExporter();
    }
}
