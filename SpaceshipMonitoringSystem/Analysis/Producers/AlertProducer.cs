using MassTransit;
using Analysis.DTO;

namespace Analysis.Producers;

public class AlertProducer : IBasicProducer<AlertMessage>
{
    private readonly IBus _bus;

    public AlertProducer(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync(AlertMessage message)
    {
        await _bus.Publish(message);
    }
}