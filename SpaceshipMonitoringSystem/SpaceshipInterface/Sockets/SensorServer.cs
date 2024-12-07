using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SpaceshipInterface.Sockets;

// Todos os sensores se conectarão ao servidor através da mesma porta
// Gerencia múltiplas conexões simultâneas usando threads assíncronas
public class SensorServer {

    private readonly int _port;
    private readonly ILogger<SensorServer> _logger;

    public SensorServer(int port, ILogger<SensorServer> logger)
    {
        _port = port;
        _logger = logger;
    }

    // Inicia o servidor e escuta conexões de clientes de forma assíncrona
    public async Task StartAsync() 
    {
        // Cria um listener na porta definida antes para aceitar conexões de qualquer endereço IP
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        _logger.LogInformation("Spaceship socket server listening on port {_port}", _port);

        // Aguarda um cliente se conectar
        while(true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    // Processa os dados recebidos do cliente conectado
    private async Task HandleClientAsync(TcpClient client)
    {
        var buffer = new byte[1024];
        var stream = client.GetStream();

        while(true)
        {
            int bytesRead;
            try
            {
                // Lê os dados enviados pelo cliente através do socket e os armazena no buffer, retorna o número de bytes lidos
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error reading data: {Message}", ex.Message);
                break;
            }

            // Verifica se o cliente se desconectou. Se sim, encerra o loop (evitar envio )
            if (bytesRead == 0)
            {
                break;
            }

            // Através do número de bytes lidos e do buffer, é possível converter os bytes correspondentes à mensagem de volta para uma string
            var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            _logger.LogInformation("Received message: {Message}", message);

            await ProcessSensorDataAsync(message);
        }

        client.Close();
    }

    // Processa os dados do sensor recebidos na mensagem
    private Task ProcessSensorDataAsync(string message)
    {
        try
        {
            // Converte a mensagem JSON em um JObject
            var json = JObject.Parse(message);
            
            foreach (var property in json.Properties())
            {
                var sensorType = property.Name;
                var value = property.Value.ToObject<object>();

                _logger.LogInformation("Sensor Type: {SensorType}, Value: {Value}", sensorType, value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing data: {Message}", ex.Message);
        }

        return Task.CompletedTask;
    }
}