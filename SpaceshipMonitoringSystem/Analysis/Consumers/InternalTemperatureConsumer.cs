using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class InternalTemperatureConsumer : IConsumer<InternalTemperatureMessage>
{
    private readonly ILogger<InternalTemperatureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;

    public InternalTemperatureConsumer(ILogger<InternalTemperatureConsumer> logger, IAnalysisService analysisService, IBasicProducer<AlertMessage> analysisProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
    }

    public async Task Consume(ConsumeContext<InternalTemperatureMessage> context)
    {
        _logger.LogInformation("Internal temperature: {Temp} ºC", context.Message.InternalTemperature);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.InternalTemperature, 12.0, 25.0);

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: Internal temperature value {Temp} ºC is out of range", context.Message.InternalTemperature );

            var alertMessage = new AlertMessage
            {
                Type = "Internal Temperature",
                Message = $"Anomaly detected: Value {context.Message.InternalTemperature} ºC is out of range."
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