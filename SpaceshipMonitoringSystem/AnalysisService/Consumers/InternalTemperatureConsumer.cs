using AnalysisService.DTO;
using MassTransit;

namespace AnalysisService.Consumers;

public class InternalTemperatureConsumer : IConsumer<InternalTemperatureMessage>
{
    private readonly ILogger<InternalPressureConsumer> _logger;

    public InternalTemperatureConsumer(ILogger<InternalPressureConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<InternalTemperatureMessage> context)
    {
        _logger.LogInformation("Internal pressure: {Pressure}", context.Message.InternalTemperature);

        return Task.CompletedTask;
    }
}