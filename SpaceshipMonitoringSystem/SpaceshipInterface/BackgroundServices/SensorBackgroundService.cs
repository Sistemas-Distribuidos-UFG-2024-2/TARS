using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

namespace SpaceshipInterface.BackgroundServices;

public class SensorBackgroundService : BackgroundService {

    private readonly ILogger<SensorBackgroundService> _logger;
    private readonly int _port = 5101; // Todos os sensores se conectarão ao servidor através da mesma porta

    public SensorBackgroundService (ILogger<SensorBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
    {

        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        _logger.LogInformation("Spaceship socket server listening on port {Port}", _port);

        try
        {
            while(!stoppingToken.IsCancellationRequested) 
            {
                // Aguarda, de forma assíncrona, até que um cliente se conecte ou que o stoppingtoken seja acionado
                var client = await listener.AcceptTcpClientAsync(stoppingToken);
                // Continua aceitando novas conexões após delegar a tarefa de lidar com o cliente, clientes processados em paralelo
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
            // É interrompido quando o serviço for encerrado
            listener.Stop();
        }
    }

    // Processa os dados recebidos de um cliente específico que esteja conectado
    private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
    {

        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8); // Leitura de dados direto do stream

        try
        {
            while (!stoppingToken.IsCancellationRequested) 
            {
                // Lê até o final da linha
                var message = await reader.ReadLineAsync();

                if (message == null)
                {
                    break; // Cliente desconectou, encerra o loop
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    continue; // Ignora mensagens em branco
                }

                await ProcessSensorData(message);
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
    private Task ProcessSensorData(string message)
    {
        try
        {
            // Converte a mensagem JSON em um Dictionary<string, object> => <propriedade, valor>
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(message);

            if (json != null) 
            {
                foreach (var property in json) 
                {
                    var sensorType = property.Key;
                    var value = property.Value;

                    var line = $"Value: {value} [{sensorType}]";

                    if (!Directory.Exists("./Logs"))
                    {
                        Directory.CreateDirectory("./Logs");
                    }

                    // Salva os dados dos sensores em um arquivo de texto
                    // Para acessar os arquivos no docker basta dar os seguintes comandos com o container em exec: cd Logs -> cat sensors_data.txt
                    File.AppendAllText("./Logs/sensors_data.txt", line + Environment.NewLine);
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