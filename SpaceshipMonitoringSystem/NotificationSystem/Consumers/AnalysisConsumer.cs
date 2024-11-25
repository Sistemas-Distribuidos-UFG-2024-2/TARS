using NotificationSystem.DTO;
using MassTransit;

namespace NotificationSystem.Consumers;

public class AnalysisConsumer : IConsumer<AlertMessage>
{
    private readonly ILogger<AnalysisConsumer> _logger;

    public AnalysisConsumer(ILogger<AnalysisConsumer> logger)
    {
        _logger = logger;
    }


    public Task Consume(ConsumeContext<AlertMessage> context)
    {
        _logger.LogInformation($"[NotificationSystem] Alert received! Type: {context.Message.Type} - Message: {context.Message.Message}");
        return Task.CompletedTask;
    }
}