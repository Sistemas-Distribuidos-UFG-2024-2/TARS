using StackExchange.Redis;

namespace HealthCheck.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _db;

    public RedisService()
    {
        var redis = ConnectionMultiplexer.Connect("redis-stack:6379");

        _db = redis.GetDatabase();
    }

    public async Task<List<string>> GetSet(string key)
    {
        var redisValues =  await _db.SetMembersAsync(key);
        
        return redisValues.Select(x => x.ToString()).ToList();
    }

    public async Task AddSetItems(string key, string[] values)
    {
        var redisKey = new RedisKey(key);

        var redisValueArray = new RedisValue[values.Length];

        for (var i = 0; i < values.Length; i++)
        {
            var redisValue = new RedisValue(values[i]);
            redisValueArray[i] = redisValue;
        }

        await _db.SetAddAsync(redisKey, redisValueArray);
    }

    public async Task AddSetItem(string key, string value)
    {
        await _db.SetAddAsync(key, value);
    }


    public async Task RemoveSetItems(string key, string[] values)
    {
        var redisValueArray = new RedisValue[values.Length];

        for (var i = 0; i < values.Length; i++)
        {
            var redisValue = new RedisValue(values[i]);
            redisValueArray[i] = redisValue;
        }

        await _db.SetRemoveAsync(new RedisKey(key), redisValueArray);
    }
}