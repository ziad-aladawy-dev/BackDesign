using System.Text.Json;
using HUP.Core.Interfaces;
using StackExchange.Redis;
namespace HUP.Application.Services.Caching;

public class CacheService: ICacheService
{
    private readonly IDatabase? _db;
    public CacheService(IServiceProvider serviceProvider)
    {
        var redis = serviceProvider.GetService<IConnectionMultiplexer>();
        _db = redis?.GetDatabase();
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        if (_db == null) return default;
        var redisValue = await _db.StringGetAsync(key);
        if (redisValue.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(redisValue.ToString());
    }

    public async Task SetAsync<T>(string key, T value, int expirationInMinutes)
    {
        if (_db == null) return;
        var json  = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, TimeSpan.FromMinutes(expirationInMinutes));
        
    }

    public async Task RemoveAsync(string key)
    {
        if (_db == null) return;
        await _db.KeyDeleteAsync(key);
    }
}
