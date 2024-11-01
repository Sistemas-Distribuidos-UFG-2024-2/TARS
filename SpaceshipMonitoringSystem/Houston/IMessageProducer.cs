namespace Houston;

public interface IMessageProducer
{
    public void SendMessage(string queue, byte[] message, CancellationToken cancellationToken = default);
}