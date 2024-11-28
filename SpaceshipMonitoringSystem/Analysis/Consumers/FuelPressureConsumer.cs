using Analysis.DTO;
using Analysis.Services;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Consumers;

public class FuelPressureConsumer : IConsumer<FuelPressureMessage>
{
    private readonly ILogger<FuelPressureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _alertProducer;

    public FuelPressureConsumer(ILogger<FuelPressureConsumer> logger, IAnalysisService analysisService, IBasicProducer<AlertMessage> alertProducer)
    {
        _logger = logger;
        _analysisService = analysisService;
        _alertProducer = alertProducer;
    }

    public async Task Consume(ConsumeContext<FuelPressureMessage> context)
    {
        _logger.LogInformation("Fuel pressure: {Pressure}", context.Message.FuelPressure);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.FuelPressure, 50.0, 150.0);

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: Fuel pressure value {Pressure} is out of range", context.Message.FuelPressure);

            var alertMessage = new AlertMessage
            {
                Type = "Fuel Pressure",
                Message = $"Anomaly detected: Value {context.Message.FuelPressure} is out of range."
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