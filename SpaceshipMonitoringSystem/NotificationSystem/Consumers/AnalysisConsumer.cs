using NotificationSystem.DTO;
using MassTransit;
using NotificationSystem.Services;

namespace NotificationSystem.Consumers;

public class AnalysisConsumer : IConsumer<AlertMessage>
{
    private readonly ILogger<AnalysisConsumer> _logger;
    private readonly IMailService _mailService;

    public AnalysisConsumer(ILogger<AnalysisConsumer> logger, IMailService mailService)
    {
        _logger = logger;
        _mailService = mailService;
    }
    
    public async Task Consume(ConsumeContext<AlertMessage> context)
    {
        _logger.LogCritical("NEW ALERT RECEIVED!!! {Message} [{Type}]", context.Message.Message, context.Message.Type);
        await _mailService.SendAlert(context.Message.Type, context.Message.Message);
    }
}