using Commom.DTO;
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
        _logger.LogInformation($"[Houston] Alert received: {context.Message.AlertType} - {context.Message.Description}");
        return Task.CompletedTask;
    }
}