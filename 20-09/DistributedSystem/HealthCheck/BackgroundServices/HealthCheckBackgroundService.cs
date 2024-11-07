using Common.Extensions;
using HealthCheck.Services;

namespace HealthCheck.BackgroundServices;

public class HealthCheckBackgroundService : BackgroundService
{
    private readonly ILogger<HealthCheckBackgroundService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;
    private int _executionCount;

    public HealthCheckBackgroundService(ILogger<HealthCheckBackgroundService> logger,
        IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{BackgroundServiceName} is running", nameof(HealthCheckBackgroundService));

        await DoWork(cancellationToken);
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{BackgroundServiceName} running", nameof(HealthCheckBackgroundService));

        var client = _httpClientFactory.CreateClient("server");

        await CheckHealth(client, cancellationToken);

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await CheckHealth(client, cancellationToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{BackgroundServiceName} is stopping", nameof(HealthCheckBackgroundService));

        await base.StopAsync(cancellationToken);
    }

    private async Task CheckHealth(HttpClient client, CancellationToken cancellationToken)
    {
        var count = Interlocked.Increment(ref _executionCount);
        var redisService = _serviceProvider.GetRequiredService<IRedisService>();
        var registeredServers = await redisService.GetSet("registered_servers");

        _logger.LogInformation("{BackgroundServiceName} is working. Count: {Count}",
            nameof(HealthCheckBackgroundService), count);

        List<string> healthyServers = [];
        List<string> unhealthyServers = [];

        await Parallel.ForEachAsync(registeredServers, cancellationToken, async (url, stoppingToken) =>
        {
            try
            {
                using var response = await client.SendRequestAsync($"{url}/_health", HttpMethod.Get,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("{Response}", await response.Content.ReadAsStringAsync(stoppingToken));

                if (response.IsSuccessStatusCode)
                {
                    healthyServers.Add(url);
                }
                else
                {
                    unhealthyServers.Add(url);
                }
            }
            catch (Exception e)
            {
                unhealthyServers.Add(url);
                _logger.LogWarning(e, "Exception thrown while trying to get health check from server {Url}", url);
            }
        });

        await redisService.RemoveSetItems("healthy_servers", unhealthyServers.ToArray());
        await redisService.AddSetItems("healthy_servers", healthyServers.ToArray());
    }
}