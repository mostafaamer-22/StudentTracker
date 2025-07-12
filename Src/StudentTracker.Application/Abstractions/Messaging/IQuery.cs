using MediatR;
using StudentTracker.Domain.Shared;


namespace Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

public interface IPaginateQuery<TResponse> : IRequest<Pagination<TResponse>>
{
}