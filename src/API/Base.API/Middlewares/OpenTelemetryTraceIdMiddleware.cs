using System.Diagnostics;

namespace Base.API.Middlewares;

public class OpenTelemetryTraceIdMiddleware(RequestDelegate next, ILogger<OpenTelemetryTraceIdMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // GET CURRENT TRACE ID FROM ACTIVITY
        var traceId = Activity.Current?.TraceId.ToString();

        // CHECK
        if (string.IsNullOrEmpty(traceId))
        {
            await next(context);
            return;
        }

        // DURING THIS SCOPE , THE TRACE ID WILL BE ATTACHED TO ALL LOGS
        // SCOPE STAY OPEN RESPONSE IS RETURNED
        using (logger.BeginScope(new Dictionary<string, object> { ["traceId"] = traceId }))
        {
            await next(context);
        }
    }
}