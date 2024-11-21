using Analysis.DTO;
using MassTransit;

namespace Analysis.Consumers;

public class FuelPressureConsumer : IConsumer<FuelPressureMessage>
{
    private readonly ILogger<FuelPressureConsumer> _logger;

    public FuelPressureConsumer(ILogger<FuelPressureConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<FuelPressureMessage> context)
    {
        _logger.LogInformation("Fuel pressure: {Pressure}", context.Message.FuelPressure);
        return Task.CompletedTask;
    }
}