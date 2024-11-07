using Common.Extensions;

namespace Server.Services;

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

    public async Task SendHi()
    {
        var url = Environment.GetEnvironmentVariable("SELF_URL")!;
        url = url.Split(";").First();

        var additionalHeaders = new Dictionary<string, string> { { "Origin", url } };
        await _client.SendRequestAsync("hi", HttpMethod.Post, headers: additionalHeaders);
    }
}