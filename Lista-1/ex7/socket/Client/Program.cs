using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var client = new UdpClient();

const string serverAddress = "127.0.0.1";
const int serverPort = 8080;

var request = new Request(65, 40);

var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
var requestJson = JsonSerializer.Serialize(request, jsonOptions);
var requestBytes = Encoding.UTF8.GetBytes(requestJson);

await client.SendAsync(requestBytes, requestBytes.Length, serverAddress, serverPort);
Console.WriteLine($"Sent: {requestJson}");

var udpReceiveResult = await client.ReceiveAsync();
var responseJson = Encoding.UTF8.GetString(udpReceiveResult.Buffer);

var response = JsonSerializer.Deserialize<Response>(responseJson, jsonOptions);

Console.WriteLine($"Received: {responseJson}");

if (response == null)
{
    Console.WriteLine("Response is null");
    return;
}

Console.WriteLine(response.CanRetire ? "You can retire!" : "You cannot retire yet.");

internal record Request(uint Age, uint ServiceTime);

internal record Response(bool CanRetire);