using AnalysisService.DTO;
using MassTransit;

namespace AnalysisService.Consumers;

public class GyroscopeConsumer : IConsumer<GyroscopyMessage>
{
    private readonly ILogger<GyroscopeConsumer> _logger;

    public GyroscopeConsumer(ILogger<GyroscopeConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<GyroscopyMessage> context)
    {
        _logger.LogInformation("Gyroscopy message X: {X}, Y:{Y}, Z: {Z}", 
            context.Message.X, context.Message.Y, context.Message.Z);
        return Task.CompletedTask;
    }
}