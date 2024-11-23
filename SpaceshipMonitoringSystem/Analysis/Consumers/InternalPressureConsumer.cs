using Analysis.DTO;
using Analysis.Services;
using MassTransit;

namespace Analysis.Consumers;

public class InternalPressureConsumer : IConsumer<InternalPressureMessage>
{
    private readonly ILogger<InternalPressureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    // private readonly INotificationService _notificationService;

    public InternalPressureConsumer(ILogger<InternalPressureConsumer> logger, IAnalysisService analysisService)
    {
        _logger = logger;
        _analysisService = analysisService;
        // _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<InternalPressureMessage> context)
    {
        _logger.LogInformation("Internal pressure: {Pressure}", context.Message.InternalPressure);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.InternalPressure, 100.0, 103.0);

        if (!isValueNormal)
        {
            // Produz uma mensagem para o serviço de notificação e para o Houston
            _logger.LogWarning("Anomaly detected: Internal pressure {Pressure} is out of range!", context.Message.InternalPressure);

            // Simula uma notificação enquanto ainda não tem isso implementado
            _logger.LogInformation("Simulated Notification: Anomaly detected in Internal Pressure!");
        }
        
        return Task.CompletedTask;
    }
}