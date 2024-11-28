using MassTransit;
using Analysis.DTO;

namespace Analysis.Producers;

public class AnalysisProducer : IBasicProducer<AlertMessage>
{
    private readonly IBus _bus;

    public AnalysisProducer(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync(AlertMessage message)
    {
        await _bus.Publish(message);
    }
}