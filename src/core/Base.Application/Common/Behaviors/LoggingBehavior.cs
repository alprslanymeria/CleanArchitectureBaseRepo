using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Base.Application.Common.Behaviors;

/// <summary>
/// LOGGING BEHAVIOR FOR MEDIATR PIPELINE.
/// LOGS REQUEST/RESPONSE INFORMATION AND EXECUTION TIME.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(

        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken

        )
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("HANDLING {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);

            stopwatch.Stop();

            logger.LogInformation("HANDLED {RequestName} IN {ElapsedMilliseconds}MS", requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(ex, "ERROR HANDLING {RequestName} AFTER {ElapsedMilliseconds}MS", requestName, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
