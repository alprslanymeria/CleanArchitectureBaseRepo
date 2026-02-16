using MediatR;

namespace Base.Application.Common.CQRS;

/// <summary>
/// HANDLER INTERFACE FOR CQRS QUERIES.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
