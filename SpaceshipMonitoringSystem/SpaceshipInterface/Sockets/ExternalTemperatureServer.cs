using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpaceshipInterface.Sockets;

public class ExternalTemperatureServer {

    private readonly int _port;

    public ExternalTemperatureServer(int port) 
    {
        _port = port;
    }

    public async Task StartAsync() 
    {
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        Console.WriteLine($"[SocketServer] External temperature server listening on port {_port}");

        while(true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        var buffer = new byte[1024];
        var stream = client.GetStream();

        Console.WriteLine("[SocketServer] Client connected");

        while(true)
        {
            int bytesRead;
            try
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SocketServer] Error reading data: {ex.Message}");
                break;
            }

            if (bytesRead == 0)
            {
                Console.WriteLine("[SocketServer] Client disconnected");
                break;
            }

            var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"[SocketServer] Received message: {message}");

            await ProcessSensorDataAsync(message);
        }

        client.Close();
    }

    private Task ProcessSensorDataAsync(string message)
    {
        Console.WriteLine($"[SocketServer] Processing data: {message}");
        return Task.CompletedTask;
    }
}