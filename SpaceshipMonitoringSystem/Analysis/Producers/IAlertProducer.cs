namespace Analysis.Producers;

public interface IAlertProducer
{
    Task PublishAsync<T>(T message) where T : notnull;
}