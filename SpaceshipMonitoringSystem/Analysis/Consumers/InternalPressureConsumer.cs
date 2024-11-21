using Analysis.DTO;
using Analysis.Services;
using MassTransit;

namespace Analysis.Consumers;

public class InternalPressureConsumer : IConsumer<InternalPressureMessage>
{
    private readonly ILogger<InternalPressureConsumer> _logger;
    private readonly IAnalysisService _analysisService;

    public InternalPressureConsumer(ILogger<InternalPressureConsumer> logger, IAnalysisService analysisService)
    {
        _logger = logger;
        _analysisService = analysisService;
    }

    public Task Consume(ConsumeContext<InternalPressureMessage> context)
    {
        _logger.LogInformation("Internal pressure: {Pressure}", context.Message.InternalPressure);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.InternalPressure, 1, 3);

        if (!isValueNormal)
        {
            // Produz uma mensagem para o serviço de notificação e para o Houston
        }
        
        return Task.CompletedTask;
    }
}