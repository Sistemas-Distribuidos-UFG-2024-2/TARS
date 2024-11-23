using Analysis.DTO;
using Analysis.Services;
using MassTransit;

namespace Analysis.Consumers;

public class ExternalTemperatureConsumer : IConsumer<ExternalTemperatureMessage>
{
    private readonly ILogger<ExternalTemperatureConsumer> _logger;
    private readonly IAnalysisService _analysisService;

    public ExternalTemperatureConsumer(ILogger<ExternalTemperatureConsumer> logger, IAnalysisService analysisService)
    {
        _logger = logger;
        _analysisService = analysisService;
    }

    public Task Consume(ConsumeContext<ExternalTemperatureMessage> context)
    {
        _logger.LogInformation("External temperature: {Temp}", context.Message.Externaltemperature);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Externaltemperature, -150.0, 150.0);

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: External temperature {Temp} is out of range!", context.Message.Externaltemperature);

            _logger.LogInformation("Simulated Notification: Anomaly detected in External Temperature!");
        }

        return Task.CompletedTask;
    }
}