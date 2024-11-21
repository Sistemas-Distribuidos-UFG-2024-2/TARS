using MassTransit;
using SpaceshipInterface.DTO;

namespace SpaceshipInterface.Consumers;

public class HoustonConsumer : IConsumer<HoustonMessage>
{
    private readonly ILogger<HoustonConsumer> _logger;

    public HoustonConsumer(ILogger<HoustonConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<HoustonMessage> context)
    {
        _logger.LogInformation("[{SentTime:HH:mm:ss}] Spaceship: {Message}", context.SentTime.ToString(), context.Message.Text);
        return Task.CompletedTask;
    }
}