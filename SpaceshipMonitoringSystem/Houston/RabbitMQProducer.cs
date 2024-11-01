using RabbitMQ.Client;

namespace Houston;

public class RabbitMQProducer : IMessageProducer, IDisposable
{
    private readonly IConnection _connection;

    public RabbitMQProducer(IConfiguration configuration)
    {
        var section = configuration.GetSection("RabbitMQ");

        var hostName = section.GetValue("HostName", "localhost");
        var port = section.GetValue("Port", 5672);

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            ClientProvidedName = "app:houston component:message-producer"
        };

        _connection = factory.CreateConnection();
    }


    public void SendMessage(string queue, byte[] message, CancellationToken cancellationToken = default)
    {
        using var channel = _connection.CreateModel();
        
        channel.QueueDeclare(queue);

        channel.BasicPublish("", queue, null, message);

        channel.Close();
    }


    public void Dispose()
    {
        _connection.Close();
    }
}