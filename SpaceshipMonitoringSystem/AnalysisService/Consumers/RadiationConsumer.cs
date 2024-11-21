using AnalysisService.DTO;
using MassTransit;

namespace AnalysisService.Consumers;

public class RadiationConsumer : IConsumer<RadiationMessage>
{
    private readonly ILogger<RadiationConsumer> _logger;

    public RadiationConsumer(ILogger<RadiationConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<RadiationMessage> context)
    {
        _logger.LogInformation("Radiation: {Rad}", context.Message.Radiation);
        return Task.CompletedTask;
    }
}