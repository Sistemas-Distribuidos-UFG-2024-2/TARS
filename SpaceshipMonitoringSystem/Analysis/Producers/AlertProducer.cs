using MassTransit;

namespace Analysis.Producers;

public class AlertProducer : IAlertProducer
{
    private readonly IBus _bus;

    public AlertProducer(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<T>(T message) where T : notnull
    {
        await _bus.Publish(message);
    }
}