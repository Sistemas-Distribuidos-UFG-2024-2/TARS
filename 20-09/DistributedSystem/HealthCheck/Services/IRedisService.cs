namespace HealthCheck.Services;

public interface IRedisService
{
    Task<List<string>> GetSet(string key);
    
    Task AddSetItems(string key, string[] values);
    Task AddSetItem(string key, string value);

    Task RemoveSetItems(string key, string[] values);
}