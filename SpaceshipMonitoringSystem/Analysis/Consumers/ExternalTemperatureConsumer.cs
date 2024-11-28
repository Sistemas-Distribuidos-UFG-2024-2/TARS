using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class ExternalTemperatureConsumer : IConsumer<ExternalTemperatureMessage>
{
    private readonly ILogger<ExternalTemperatureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;

    public ExternalTemperatureConsumer(ILogger<ExternalTemperatureConsumer> logger, IAnalysisService analysisService, IBasicProducer<AlertMessage> analysisProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
    }

    public async Task Consume(ConsumeContext<ExternalTemperatureMessage> context)
    {
        _logger.LogInformation("External temperature: {Temp} ºC", context.Message.Externaltemperature);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Externaltemperature, -150.0, 150.0);

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: External temperature value {Temp} ºC is out of range", context.Message.Externaltemperature);

            var alertMessage = new AlertMessage
            {
                Type = "External Temperature",
                Message = $"Anomaly detected: Value {context.Message.Externaltemperature} ºC is out of range."
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