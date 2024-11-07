namespace Client.Services;

public interface IHealthCheckService
{
    Task<string[]> GetHealthyServers();
}