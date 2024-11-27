using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class ExternalTemperatureConsumer : IConsumer<ExternalTemperatureMessage>
{
    private readonly ILogger<ExternalTemperatureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IAlertProducer _alertProducer;

    public ExternalTemperatureConsumer(ILogger<ExternalTemperatureConsumer> logger, IAnalysisService analysisService, IAlertProducer alertProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _alertProducer = alertProducer;
    }

    public async Task Consume(ConsumeContext<ExternalTemperatureMessage> context)
    {
        _logger.LogInformation("External temperature: {Temp}", context.Message.Externaltemperature);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Externaltemperature, -150.0, 150.0);

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: External temperature value {Temp} is out of range", context.Message.Externaltemperature);

            var alertMessage = new AlertMessage
            {
                Type = "External Temperature",
                Message = $"Anomaly detected: Value {context.Message.Externaltemperature} is out of range."
            };

            try
            {
                await _alertProducer.PublishAsync(alertMessage);
                _logger.LogInformation("Alert message sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send alert message");
            }
        }
    }
}