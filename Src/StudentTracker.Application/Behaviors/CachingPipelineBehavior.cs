using MediatR;
using StudentTracker.Application.Abstractions.Caching;
using StudentTracker.Application.Abstractions.Messaging;
using StudentTracker.Domain.Shared;
using System.Text.Json;

namespace StudentTracker.Application.Behaviors;

public class CachingPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
    where TResponse : Result
{
    private readonly ICacheService _cache;

    public CachingPipelineBehavior(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is ICommand or ICommand<TResponse>)
        {
            await _cache.ClearAllAsync();
        }

        string requestData = JsonSerializer.Serialize(request);
        var cacheKey = $"{typeof(TRequest).Name}_{ComputeHash(requestData)}";

        var cachedResponse = await _cache.GetAsync<TResponse>(cacheKey);
        if (cachedResponse != null)
            return cachedResponse;

        var response = await next();

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromSeconds(15));

        return response;
    }

    private static string ComputeHash(string data)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(data);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}