using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class InternalTemperatureConsumer : IConsumer<InternalTemperatureMessage>
{
    private readonly ILogger<InternalPressureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IAlertProducer _alertProducer;

    public InternalTemperatureConsumer(ILogger<InternalPressureConsumer> logger, IAnalysisService analysisService, IAlertProducer alertProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _alertProducer = alertProducer;
    }

    public async Task Consume(ConsumeContext<InternalTemperatureMessage> context)
    {
        _logger.LogInformation("Internal temperature: {Temp}", context.Message.InternalTemperature);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.InternalTemperature, 12.0, 25.0);

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: Internal temperature {Temp} is out of range", context.Message.InternalTemperature );

            var alertMessage = new AlertMessage
            {
                Type = "Internal Temperature",
                Message = $"Anomaly detected: Value {context.Message.InternalTemperature} is out of range."
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