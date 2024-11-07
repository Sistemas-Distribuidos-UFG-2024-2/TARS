using MassTransit;
using NotificationSystem.DTO;

namespace NotificationSystem.BackgroundServices;

public class Producer : BackgroundService
{
    private readonly IBus _bus;

    public Producer(IBus bus)
    {
        _bus = bus;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new BasicMessage
            {
                State = "Chaos"
            }, stoppingToken);
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}