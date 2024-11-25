using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class RadiationConsumer : IConsumer<RadiationMessage>
{
    private readonly ILogger<RadiationConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IAlertProducer _alertProducer;

    public RadiationConsumer(ILogger<RadiationConsumer> logger, IAnalysisService analysisService, IAlertProducer alertProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _alertProducer = alertProducer;
    }

    public async Task Consume(ConsumeContext<RadiationMessage> context)
    {
        _logger.LogInformation("Radiation: {Rad}", context.Message.Radiation);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Radiation, 50.0, 500.0);

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Radiation {Rad} is out of range", context.Message.Radiation);

            var alertMessage = new AlertMessage
            {
                Type = "Radiation",
                Message = $"Anomaly detected: Value {context.Message.Radiation} is out of range."
            };

            try
            {
                await _alertProducer.PublishAsync(alertMessage);
                _logger.LogInformation("[Analysis]: Alert message sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Analysis]: Failed to send alert message");
            }
        }
    }
}