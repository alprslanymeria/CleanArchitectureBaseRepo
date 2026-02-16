using MediatR;

namespace Base.Application.Common.CQRS;

/// <summary>
/// MARKER INTERFACE FOR CQRS COMMANDS THAT RETURN A RESPONSE.
/// COMMANDS ARE WRITE OPERATIONS THAT MODIFY STATE.
/// </summary>
public interface ICommand<TResponse> : IRequest<TResponse>;

/// <summary>
/// MARKER INTERFACE FOR CQRS COMMANDS THAT DO NOT RETURN A RESPONSE.
/// COMMANDS ARE WRITE OPERATIONS THAT MODIFY STATE.
/// </summary>
public interface ICommand : IRequest;
