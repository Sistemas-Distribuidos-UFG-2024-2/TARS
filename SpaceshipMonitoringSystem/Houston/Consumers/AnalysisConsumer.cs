using Houston.DTO;
using MassTransit;

namespace Houston.Consumers;

public class AnalysisConsumer : IConsumer<AlertMessage>
{
    private readonly ILogger<AnalysisConsumer> _logger;

    public AnalysisConsumer(ILogger<AnalysisConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<AlertMessage> context)
    {
        _logger.LogInformation($"[Houston] Alert received! Type: {context.Message.Type} - Message: {context.Message.Message}");
        return Task.CompletedTask;
    }
}