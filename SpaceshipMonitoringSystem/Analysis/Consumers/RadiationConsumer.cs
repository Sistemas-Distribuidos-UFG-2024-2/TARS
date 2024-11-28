using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class RadiationConsumer : IConsumer<RadiationMessage>
{
    private readonly ILogger<RadiationConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;

    public RadiationConsumer(ILogger<RadiationConsumer> logger, IAnalysisService analysisService, IBasicProducer<AlertMessage> analysisProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
    }

    public async Task Consume(ConsumeContext<RadiationMessage> context)
    {
        _logger.LogInformation("Radiation: {Rad} μSv/h", context.Message.Radiation);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Radiation, 50.0, 500.0);

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Radiation value {Rad} μSv/h is out of range", context.Message.Radiation);

            var alertMessage = new AlertMessage
            {
                Type = "Radiation",
                Message = $"Anomaly detected: Value {context.Message.Radiation} μSv/h is out of range."
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