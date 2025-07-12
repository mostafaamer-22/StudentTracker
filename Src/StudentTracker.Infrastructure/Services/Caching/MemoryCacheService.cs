using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using StudentTracker.Application.Abstractions.Caching;

namespace StudentTracker.Infrastructure.Services.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentBag<string> _keys = new();

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        var value = _memoryCache.TryGetValue(key, out T? result) ? result : default;
        return Task.FromResult(value);
    }

    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        _keys.TryTake(out _);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        _memoryCache.Set(key, value, options);
        _keys.Add(key);
        return Task.CompletedTask;
    }

    public Task ClearAllAsync()
    {
        foreach (var key in _keys)
        {
            _memoryCache.Remove(key);
        }
        _keys.Clear();
        return Task.CompletedTask;
    }
}
