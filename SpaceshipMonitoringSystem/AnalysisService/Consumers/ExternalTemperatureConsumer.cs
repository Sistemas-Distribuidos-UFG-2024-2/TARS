using MassTransit;

namespace AnalysisService.Consumers;

public class ExternalTemperatureConsumer : IConsumer<object>
{
    private readonly ILogger<ExternalTemperatureConsumer> _logger;

    public ExternalTemperatureConsumer(ILogger<ExternalTemperatureConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<object> context)
    {
        _logger.LogInformation("Message: {State}", context.Message);
        
        return Task.CompletedTask;
    }
}