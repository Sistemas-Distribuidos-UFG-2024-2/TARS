using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class AccelerationConsumer : IConsumer<AccelerationMessage>
{
    private readonly ILogger<AccelerationConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;

    public AccelerationConsumer(ILogger<AccelerationConsumer> logger, IAnalysisService analysisService, IBasicProducer<AlertMessage> analysisProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
    }

    public async Task Consume(ConsumeContext<AccelerationMessage> context)
    {
        _logger.LogInformation("Acceleration: {Acceleration} µm", context.Message.Acceleration);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Acceleration, -1.0, 1.0);

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Acceleration value {Acceleration} µm is out of range", context.Message.Acceleration);

            var alertMessage = new AlertMessage
            {
                Type = "Acceleration",
                Message = $"Anomaly detected: Value {context.Message.Acceleration} µm is out of range."
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