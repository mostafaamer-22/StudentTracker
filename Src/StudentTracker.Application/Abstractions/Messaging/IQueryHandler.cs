using MediatR;
using StudentTracker.Domain.Shared;

namespace Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}

public interface IPaginateQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Pagination<TResponse>>
    where TQuery : IPaginateQuery<TResponse>
{
}