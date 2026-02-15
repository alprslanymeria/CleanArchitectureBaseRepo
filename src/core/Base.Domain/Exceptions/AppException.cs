using System.Net;

namespace Base.Domain.Exceptions;

/// <summary>
/// BASE APPLICATION EXCEPTION WITH HTTP STATUS CODE.
/// ALL CUSTOM APPLICATION EXCEPTIONS SHOULD INHERIT FROM THIS CLASS.
/// </summary>
public abstract class AppException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
