namespace Analysis.Producers;

public interface IBasicProducer<in TMessage> where TMessage : notnull
{
    Task PublishAsync(TMessage message);
}