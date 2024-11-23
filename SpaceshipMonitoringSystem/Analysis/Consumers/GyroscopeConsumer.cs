using Analysis.DTO;
using MassTransit;

namespace Analysis.Consumers;

public class GyroscopeConsumer : IConsumer<GyroscopeMessage>
{
    private readonly ILogger<GyroscopeConsumer> _logger;

    public GyroscopeConsumer(ILogger<GyroscopeConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<GyroscopeMessage> context)
    {
        _logger.LogInformation("Gyroscope message X: {X}, Y:{Y}, Z: {Z}", 
            context.Message.X, context.Message.Y, context.Message.Z);
        return Task.CompletedTask;
    }
}