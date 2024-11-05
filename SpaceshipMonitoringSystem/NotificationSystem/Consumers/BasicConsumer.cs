using MassTransit;
using NotificationSystem.DTO;

namespace NotificationSystem.Consumers;

public class BasicConsumer : IConsumer<BasicMessage>
{
    private readonly ILogger<BasicConsumer> _logger;

    public BasicConsumer(ILogger<BasicConsumer> logger)
    {
        _logger = logger;
    }


    public Task Consume(ConsumeContext<BasicMessage> context)
    {
        _logger.LogInformation("STATE: {State}", context.Message.State);
        
        return Task.CompletedTask;
    }
}