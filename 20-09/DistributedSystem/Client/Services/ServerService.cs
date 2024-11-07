using System.Text;
using System.Text.Json;
using Common;
using Common.Extensions;

namespace Client.Services;

public class ServerService : IServerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<ServerService> _logger;

    public ServerService(IHttpClientFactory httpClientFactory, IHealthCheckService healthCheckService,
        ILogger<ServerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    public async Task<SumResponse?> GetServerResponse(SumRequest request)
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

            using var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            using var response =
                await client.SendRequestAsync(healthyServers[index] + "/receive", HttpMethod.Post, content);

            return await response.Content.ReadFromJsonAsync<SumResponse>();
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "Error while getting server response");
            throw;
        }
    }
}