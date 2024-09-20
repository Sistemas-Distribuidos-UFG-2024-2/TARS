using Common.Extensions;

namespace Client.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly HttpClient _client;

    public HealthCheckService(HttpClient client, IConfiguration configuration)
    {
        _client = client;

        var section = configuration.GetSection("HealthCheck");

        _client.BaseAddress = new Uri(section.GetValue<string>("BaseAddress")!);
        _client.Timeout = TimeSpan.FromSeconds(section.GetValue<int>("Timeout"));
    }

    public async Task<string[]> GetHealthyServers()
    {
        using var response = await _client.SendRequestAsync("get-healthy-servers", HttpMethod.Get);

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        var healthyServers = await response.Content.ReadFromJsonAsync<string[]>();

        return healthyServers ?? [];
    }
}