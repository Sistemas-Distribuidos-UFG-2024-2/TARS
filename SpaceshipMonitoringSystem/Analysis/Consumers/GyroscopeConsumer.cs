using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class GyroscopeConsumer : IConsumer<GyroscopeMessage>
{
    private readonly ILogger<GyroscopeConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;

    public GyroscopeConsumer(ILogger<GyroscopeConsumer> logger, IAnalysisService analysisService, IBasicProducer<AlertMessage> analysisProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
    }

    public async Task Consume(ConsumeContext<GyroscopeMessage> context)
    {
        _logger.LogInformation
        (
            "Gyroscope message X: {X} °/s, Y:{Y} °/s, Z: {Z} °/s", 
            context.Message.X, 
            context.Message.Y, 
            context.Message.Z
        );
        
        // Os 3 eixos são independentes
        var axes = new Dictionary<string, double>
        {
            { "X", context.Message.X },
            { "Y", context.Message.Y },
            { "Z", context.Message.Z }
        };

        foreach (var (axis, value) in axes)
        {
            var isValueNormal = _analysisService.IsValueNormal(value, -5.0, 5.0);
            
            if (!isValueNormal)
            {
                _logger.LogWarning("Anomaly detected: Gyroscope's axis {Axis} value {Value} °/s is out of range", axis, value);

                var alertMessage = new AlertMessage
                {
                    Type = "Gyroscope",
                    Message = $"Anomaly detected: Axis {axis} value {value} °/s is out of range."
                };

                try
                {
                    await _analysisProducer.PublishAsync(alertMessage);
                    _logger.LogInformation("Alert message sent successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send alert message");
                }
            }
        }
    }
}