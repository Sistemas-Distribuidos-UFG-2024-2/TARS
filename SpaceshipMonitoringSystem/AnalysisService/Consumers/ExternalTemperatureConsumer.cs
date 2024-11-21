using AnalysisService.DTO;
using MassTransit;

namespace AnalysisService.Consumers;

public class ExternalTemperatureConsumer : IConsumer<ExternalTemperatureMessage>
{
    private readonly ILogger<ExternalTemperatureConsumer> _logger;

    public ExternalTemperatureConsumer(ILogger<ExternalTemperatureConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ExternalTemperatureMessage> context)
    {
        _logger.LogInformation("External temperature: {Temp}", context.Message.Externaltemperature);

        return Task.CompletedTask;
    }
}