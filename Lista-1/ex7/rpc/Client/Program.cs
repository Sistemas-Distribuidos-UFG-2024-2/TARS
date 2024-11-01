
using Grpc.Net.Client;
using Retirement;

using var channel = GrpcChannel.ForAddress($"http://[::1]:50051");

var client = new Retirement.Retirement.RetirementClient(channel);

var reply = client.CanRetire(new RetirementRequest
{
    Age = 50, ServiceTime = 20
}); 

Console.WriteLine("Can retire? {0}" , reply.CanRetire ? "Yes" : "No");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
