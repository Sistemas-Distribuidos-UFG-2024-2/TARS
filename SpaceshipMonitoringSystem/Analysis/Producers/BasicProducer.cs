using MassTransit;

namespace Analysis.Producers;

public class BasicProducer : IBasicProducer
{
    private readonly IBus _bus;

    public BasicProducer(IBus bus)
    {
        _bus = bus;
    }
    
    public string QueueName => "queue:basic-queue";

    public async Task PublishAsync<T>(T message) where T : notnull
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri(QueueName));

        await endpoint.Send(message);
    }
}