using Houston.DTO;
using MassTransit;

namespace Houston.Producers;

public class SpaceshipProducer : IBasicProducer<SpaceshipMessage>
{
    private readonly IBus _bus;

    public SpaceshipProducer(IBus bus)
    {
        _bus = bus;
    }

    public string QueueName => "queue:spaceship";
    
    public async Task PublishAsync(SpaceshipMessage message)
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri(QueueName));

        await endpoint.Send(message);
    }
}