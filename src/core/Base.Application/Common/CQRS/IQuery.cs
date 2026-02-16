using MediatR;

namespace Base.Application.Common.CQRS;

/// <summary>
/// MARKER INTERFACE FOR CQRS QUERIES.
/// QUERIES ARE READ OPERATIONS THAT DO NOT MODIFY STATE.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>;
