using Analysis.DTO;
using Analysis.Services;
using MassTransit;

namespace Analysis.Consumers;

public class RadiationConsumer : IConsumer<RadiationMessage>
{
    private readonly ILogger<RadiationConsumer> _logger;
    private readonly IAnalysisService _analysisService;

    public RadiationConsumer(ILogger<RadiationConsumer> logger, IAnalysisService analysisService)
    {
        _logger = logger;
        _analysisService = analysisService;
    }

    public Task Consume(ConsumeContext<RadiationMessage> context)
    {
        _logger.LogInformation("Radiation: {Rad}", context.Message.Radiation);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Radiation, 50.0, 500.0);

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Radiation {Rad} is out of range!", context.Message.Radiation);

            _logger.LogInformation("Simulated Notification: Anomaly detected in Radiation!");
        }

        return Task.CompletedTask;
    }
}