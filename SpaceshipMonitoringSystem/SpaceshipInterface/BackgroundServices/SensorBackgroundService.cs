using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO; 

namespace SpaceshipInterface.BackgroundServices;

// Todos os sensores se conectarão ao servidor através da mesma porta
// Gerencia múltiplas conexões simultâneas usando threads assíncronas
public class SensorBackgroundService : BackgroundService {

    private readonly ILogger<SensorBackgroundService> _logger;
    private readonly int _port = 5101;

    public SensorBackgroundService (ILogger<SensorBackgroundService> logger)
    {
        _logger = logger;
    }

    // Inicia o servidor e escuta conexões de clientes de forma assíncrona em segundo plano
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
    {

        // Cria um listener na porta definida antes para aceitar conexões de qualquer endereço IP
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        _logger.LogInformation("Spaceship socket server listening on port {Port}", _port);

        try
        {
            while(!stoppingToken.IsCancellationRequested) 
            {
                var client = await listener.AcceptTcpClientAsync(stoppingToken);
                _ = HandleClientAsync(client, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Spaceship socket server shutting down...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while executing the sensor background service");
        }
        finally
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            listener.Stop();
        }
    }

    // Processa os dados recebidos de um único cliente conectado
    private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
    {

        using var stream = client.GetStream(); // Garantir descarte correto do stream
        using var reader = new StreamReader(stream, Encoding.UTF8); // Leitura direto do stream

        try
        {
            while (!stoppingToken.IsCancellationRequested) 
            {
                // Lê até o final da linha (retorna null quando alcança o fim do fluxo ou quando o cliente encerra a conexão)
                var message = await reader.ReadLineAsync();

                if (message == null)
                {
                    break; // Cliente desconectou, encerra o loop
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    continue; // Ignora mensagens em branco
                }

                await ProcessSensorDataAsync(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sensor data");
        }
        finally
        {
            client.Close();
        }
    }

    // Processa os dados do sensor recebidos na mensagem
    private Task ProcessSensorDataAsync(string message)
    {
        try
        {
            // Converte a mensagem JSON em um Dictionary<string, object> => propriedade, valor
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(message);

            if (json != null) 
            {
                foreach (var property in json) 
                {
                    var sensorType = property.Key;
                    var value = property.Value;

                    _logger.LogInformation("Sensor: {SensorType}, Value: {Value}", sensorType, value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading data from sensor");
        }

        return Task.CompletedTask;
    }
}