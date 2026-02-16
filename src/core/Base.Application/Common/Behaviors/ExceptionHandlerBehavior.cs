using Base.Application.Common;
using Base.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Base.Application.Common.Behaviors;

/// <summary>
/// EXCEPTION HANDLER BEHAVIOR FOR MEDIATR PIPELINE.
/// CATCHES EXCEPTIONS THROWN BY HANDLERS AND CONVERTS THEM INTO SERVICERESULT RESPONSES.
/// LOGS THE HANDLER NAME AND ERROR DETAILS FOR TRACEABILITY.
/// </summary>
public class ExceptionHandlerBehavior<TRequest, TResponse>(ILogger<ExceptionHandlerBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{

    #region UTILS

    private static TResponse CreateFailResponse(string errorMessage, HttpStatusCode statusCode)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(ServiceResult))
        {
            return (TResponse)(object)ServiceResult.Fail(errorMessage, statusCode);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ServiceResult<>))
        {
            var failMethod = responseType.GetMethod(nameof(ServiceResult.Fail), [typeof(string), typeof(HttpStatusCode)])!;
            return (TResponse)failMethod.Invoke(null, [errorMessage, statusCode])!;
        }

        throw new InvalidOperationException($"UNSUPPORTED RESPONSE TYPE: {responseType.Name}");
    }

    #endregion

    public async Task<TResponse> Handle(

        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken

        )
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (AppException ex)
        {
            var handlerName = typeof(TRequest).Name;

            logger.LogWarning(ex, "{HandlerName} -> {ErrorMessage}", handlerName, ex.Message);

            return CreateFailResponse(ex.Message, ex.StatusCode);
        }
        catch (Exception ex)
        {
            var handlerName = typeof(TRequest).Name;

            logger.LogError(ex, "{HandlerName} -> AN UNEXPECTED ERROR OCCURRED", handlerName);

            return CreateFailResponse($"AN UNEXPECTED ERROR OCCURRED: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
