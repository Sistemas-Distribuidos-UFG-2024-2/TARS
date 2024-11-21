namespace Houston.Producers;

public interface IBasicProducer<in TMessage> where TMessage : notnull
{
    string QueueName { get; }
    Task PublishAsync(TMessage message);
}