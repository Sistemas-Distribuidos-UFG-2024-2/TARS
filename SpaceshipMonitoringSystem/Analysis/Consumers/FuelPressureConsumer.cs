using Analysis.DTO;
using Analysis.Services;
using MassTransit;

namespace Analysis.Consumers;

public class FuelPressureConsumer : IConsumer<FuelPressureMessage>
{
    private readonly ILogger<FuelPressureConsumer> _logger;
    private readonly IAnalysisService _analysisService;

    public FuelPressureConsumer(ILogger<FuelPressureConsumer> logger, IAnalysisService analysisService)
    {
        _logger = logger;
        _analysisService = analysisService;
    }

    public Task Consume(ConsumeContext<FuelPressureMessage> context)
    {
        _logger.LogInformation("Fuel pressure: {Pressure}", context.Message.FuelPressure);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.FuelPressure, 50.0, 150.0);

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: Fuel pressure {Pressure} is out of range!", context.Message.FuelPressure);

            _logger.LogInformation("Simulated Notification: Anomaly detected in Spaceship Fuel Pressure!");
        }

        return Task.CompletedTask;
    }
}