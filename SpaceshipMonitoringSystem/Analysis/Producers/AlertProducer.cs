using MassTransit;

namespace Analysis.Producers;

public class AlertProducer : IAlertProducer
{
    private readonly IBus _bus;

    public AlertProducer(IBus bus)
    {
        _bus = bus;
    }

    public string QueueName => "queue:alerts";

    public async Task PublishAsync<T>(T message) where T : notnull
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri(QueueName));
        await endpoint.Send(message);
    }
}