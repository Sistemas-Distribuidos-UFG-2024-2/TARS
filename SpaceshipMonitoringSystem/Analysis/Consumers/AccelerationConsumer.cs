using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class AccelerationConsumer : IConsumer<AccelerationMessage>
{
    private readonly ILogger<AccelerationConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IAlertProducer _alertProducer;

    public AccelerationConsumer(ILogger<AccelerationConsumer> logger, IAnalysisService analysisService, IAlertProducer alertProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _alertProducer = alertProducer;
    }

    public async Task Consume(ConsumeContext<AccelerationMessage> context)
    {
        _logger.LogInformation("Acceleration: {Acceleration}", context.Message.Acceleration);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Acceleration, -1.0, 1.0);

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Acceleration {Acceleration} is out of range", context.Message.Acceleration);

            var alertMessage = new AlertMessage
            {
                Type = "Acceleration",
                Message = $"Anomaly detected: Value {context.Message.Acceleration} is out of range."
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