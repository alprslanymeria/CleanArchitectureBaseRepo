using System.Net;

namespace Base.Domain.Exceptions;

/// <summary>
/// EXCEPTION THROWN WHEN A REQUESTED ENTITY IS NOT FOUND IN THE DATABASE.
/// </summary>
public sealed class NotFoundException(string message) : AppException(message, HttpStatusCode.NotFound);
