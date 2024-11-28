using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class InternalPressureConsumer : IConsumer<InternalPressureMessage>
{
    private readonly ILogger<InternalPressureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;

    public InternalPressureConsumer(ILogger<InternalPressureConsumer> logger, IAnalysisService analysisService, IBasicProducer<AlertMessage> analysisProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
    }

    public async Task Consume(ConsumeContext<InternalPressureMessage> context)
    {
        _logger.LogInformation("Internal pressure: {Pressure} kPa", context.Message.InternalPressure);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.InternalPressure, 100.0, 103.0);

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Internal pressure value {Pressure} kPa is out of range", context.Message.InternalPressure);

            var alertMessage = new AlertMessage
            {
                Type = "Internal Pressure",
                Message = $"Anomaly detected: Value {context.Message.InternalPressure} kPa is out of range."
            };

            // Produz uma mensagem de alerta para o serviço de notificação e para o Houston
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