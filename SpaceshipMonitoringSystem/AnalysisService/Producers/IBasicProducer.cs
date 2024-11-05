namespace AnalysisService.Producers;

public interface IBasicProducer
{
    string QueueName { get; }
    Task PublishAsync<T>(T message) where T : notnull;
}