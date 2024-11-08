using AnalysisService.DTO;
using MassTransit;

namespace AnalysisService.Consumers;

public class InternalPressureConsumer : IConsumer<InternalPressureMessage>
{
    private readonly ILogger<InternalPressureConsumer> _logger;

    public InternalPressureConsumer(ILogger<InternalPressureConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<InternalPressureMessage> context)
    {
        _logger.LogInformation("Internal pressure: {Pressure}", context.Message.InternalPressure);

        return Task.CompletedTask;
    }
}