using AnalysisService.DTO;
using MassTransit;

namespace AnalysisService.Consumers;

public class AccelerationConsumer : IConsumer<AccelerationMessage>
{
    private readonly ILogger<AccelerationConsumer> _logger;

    public AccelerationConsumer(ILogger<AccelerationConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<AccelerationMessage> context)
    {
        _logger.LogInformation("Acceleration: {Acceleration}", context.Message.Acceleration);

        return Task.CompletedTask;
    }
}