using Analysis.DTO;
using Analysis.Services;
using MassTransit;

namespace Analysis.Consumers;

public class GyroscopeConsumer : IConsumer<GyroscopeMessage>
{
    private readonly ILogger<GyroscopeConsumer> _logger;
    private readonly IAnalysisService _analysisService;

    public GyroscopeConsumer(ILogger<GyroscopeConsumer> logger, IAnalysisService analysisService)
    {
        _logger = logger;
        _analysisService = analysisService;
    }

    public Task Consume(ConsumeContext<GyroscopeMessage> context)
    {
        _logger.LogInformation("Gyroscope message X: {X}, Y:{Y}, Z: {Z}", 
            context.Message.X, context.Message.Y, context.Message.Z);
        
        var isValueNormal = _analysisService.IsValueNormal(context.Message.X, -5.0, 5.0);

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: Gyroscope's axis X {X} is out of range!", context.Message.X);

            _logger.LogInformation("Simulated Notification: Anomaly detected in Gyroscope's axis X!");
        }

        isValueNormal = _analysisService.IsValueNormal(context.Message.Y, -5.0, 5.0);

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Gyroscope's axis Y {Y} is out of range!", context.Message.Y);

            _logger.LogInformation("Simulated Notification: Anomaly detected in Gyroscope's axis Y!");
        }

        isValueNormal = _analysisService.IsValueNormal(context.Message.Z, -5.0, 5.0);

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Gyroscope's axis Z {Z} is out of range!", context.Message.Z);

            _logger.LogInformation("Simulated Notification: Anomaly detected in Gyroscope's axis Z!");
        }
        
        return Task.CompletedTask;
    }
}