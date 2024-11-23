using Analysis.DTO;
using Analysis.Services;
using MassTransit;

namespace Analysis.Consumers;

public class InternalTemperatureConsumer : IConsumer<InternalTemperatureMessage>
{
    private readonly ILogger<InternalPressureConsumer> _logger;
    private readonly IAnalysisService _analysisService;

    public InternalTemperatureConsumer(ILogger<InternalPressureConsumer> logger, IAnalysisService analysisService)
    {
        _logger = logger;
        _analysisService = analysisService;
    }

    public Task Consume(ConsumeContext<InternalTemperatureMessage> context)
    {
        _logger.LogInformation("Internal temperature: {Temp}", context.Message.InternalTemperature);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.InternalTemperature, 12.0, 25.0);

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: Internal temperature {Temp} is out of range!", context.Message.InternalTemperature );

            _logger.LogInformation("Simulated Notification: Anomaly detected in Spaceship Internal Temperature!");
        }

        return Task.CompletedTask;
    }
}