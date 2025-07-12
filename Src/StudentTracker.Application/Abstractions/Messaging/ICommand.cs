using MediatR;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
