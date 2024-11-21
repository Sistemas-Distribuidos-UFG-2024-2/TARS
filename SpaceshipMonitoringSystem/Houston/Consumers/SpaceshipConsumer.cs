using Houston.DTO;
using MassTransit;

namespace Houston.Consumers;

public class SpaceshipConsumer : IConsumer<SpaceshipMessage>
{
    private readonly ILogger<SpaceshipConsumer> _logger;

    public SpaceshipConsumer(ILogger<SpaceshipConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SpaceshipMessage> context)
    {
        _logger.LogInformation("[{SentTime:HH:mm:ss}] Spaceship: {Message}", context.SentTime.ToString(), context.Message.Text);
        return Task.CompletedTask;
    }
}