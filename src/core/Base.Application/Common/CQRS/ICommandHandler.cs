using MediatR;

namespace Base.Application.Common.CQRS;

/// <summary>
/// HANDLER INTERFACE FOR CQRS COMMANDS THAT RETURN A RESPONSE.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>;

/// <summary>
/// HANDLER INTERFACE FOR CQRS COMMANDS THAT DO NOT RETURN A RESPONSE.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand;
