using SpaceshipInterface.DTO;
using MassTransit;

namespace SpaceshipInterface.Consumers;

public class AnalysisConsumer : IConsumer<AlertMessage>
{
    private readonly ILogger<AnalysisConsumer> _logger;

    public AnalysisConsumer(ILogger<AnalysisConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<AlertMessage> context)
    {
        _logger.LogCritical($"NEW ALERT RECEIVED!!! {context.Message.Message} [{context.Message.Type}]");
        return Task.CompletedTask;
    }
}