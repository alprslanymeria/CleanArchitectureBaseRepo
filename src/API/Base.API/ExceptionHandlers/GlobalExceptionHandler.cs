using Base.Application;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using Base.Application.Common;

namespace Base.API.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var errorAsDto = ServiceResult.Fail(exception.Message, HttpStatusCode.InternalServerError);

        httpContext.Response.StatusCode = HttpStatusCode.InternalServerError.GetHashCode();
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(errorAsDto, cancellationToken);

        return true;

        // return true;  --> Bu hatayı ben ele aldım ve ilgili response modeli ben geri döneceğim.
        // return false; --> Bu hatayı ele aldım, gerekli operasyonlarımı yaptım. Bundan sonraki yolculuğuna devam etsin.
    }
}
