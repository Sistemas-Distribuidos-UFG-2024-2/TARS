using Analysis.DTO;
using Analysis.Services;
using MassTransit;

namespace Analysis.Consumers;

public class AccelerationConsumer : IConsumer<AccelerationMessage>
{
    private readonly ILogger<AccelerationConsumer> _logger;
    private readonly IAnalysisService _analysisService;

    public AccelerationConsumer(ILogger<AccelerationConsumer> logger, IAnalysisService analysisService)
    {
        _logger = logger;
        _analysisService = analysisService;
    }

    public Task Consume(ConsumeContext<AccelerationMessage> context)
    {
        _logger.LogInformation("Acceleration: {Acceleration}", context.Message.Acceleration);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Acceleration, -1.0, 1.0);

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Acceleration {Acceleration} is out of range!", context.Message.Acceleration);

            _logger.LogInformation("Simulated Notification: Anomaly detected in Spaceship Acceleration!");
        }

        return Task.CompletedTask;
    }
}