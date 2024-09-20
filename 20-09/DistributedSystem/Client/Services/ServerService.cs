using Common.Extensions;

namespace Client.Services;

public class ServerService : IServerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<ServerService> _logger;

    public ServerService(IHttpClientFactory httpClientFactory, IHealthCheckService healthCheckService, ILogger<ServerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    public async Task<string?> GetServerResponse()
    {
        try
        {
            var healthyServers = await _healthCheckService.GetHealthyServers();

            if (healthyServers.Length == 0)
            {
                throw new ApplicationException("No healthy servers found");
            }

            var index = Random.Shared.Next(0, healthyServers.Length - 1);
            using var client = _httpClientFactory.CreateClient("server");
            
            using var response = await client.SendRequestAsync(healthyServers[index] + "/receive", HttpMethod.Get);

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "Error while getting server response");
            throw;
        }
    }
}