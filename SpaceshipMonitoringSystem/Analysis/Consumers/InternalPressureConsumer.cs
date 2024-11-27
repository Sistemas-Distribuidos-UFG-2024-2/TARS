using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class InternalPressureConsumer : IConsumer<InternalPressureMessage>
{
    private readonly ILogger<InternalPressureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IAlertProducer _alertProducer;

    public InternalPressureConsumer(ILogger<InternalPressureConsumer> logger, IAnalysisService analysisService, IAlertProducer alertProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _alertProducer = alertProducer;
    }

    public async Task Consume(ConsumeContext<InternalPressureMessage> context)
    {
        _logger.LogInformation("Internal pressure: {Pressure}", context.Message.InternalPressure);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.InternalPressure, 100.0, 103.0);

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Internal pressure value {Pressure} is out of range", context.Message.InternalPressure);

            var alertMessage = new AlertMessage
            {
                Type = "Internal Pressure",
                Message = $"Anomaly detected: Value {context.Message.InternalPressure} is out of range."
            };

            // Produz uma mensagem de alerta para o serviço de notificação e para o Houston
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