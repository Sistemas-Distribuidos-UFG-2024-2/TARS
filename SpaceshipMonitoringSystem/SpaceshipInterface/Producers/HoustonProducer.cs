using MassTransit;
using SpaceshipInterface.DTO;

namespace SpaceshipInterface.Producers;

public class HoustonProducer : IBasicProducer<HoustonMessage>
{
    private readonly IBus _bus;

    public HoustonProducer(IBus bus)
    {
        _bus = bus;
    }

    public string QueueName => "queue:houston";
    
    public async Task PublishAsync(HoustonMessage message)
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri(QueueName));

        await endpoint.Send(message);
    }
}