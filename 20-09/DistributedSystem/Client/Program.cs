using Client.Extensions;
using Client.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureLogging();
builder.Services.AddServices();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => TypedResults.Ok("Client is up and running!"))
    .WithName("Home")
    .WithOpenApi();


app.MapGet("/send", GetServerResponse)
    .WithName("Send")
    .WithOpenApi();

app.Run();

return;

async Task<IResult> GetServerResponse([FromKeyedServices("server")] IServerService serverService){
    try
    {
        var response = await serverService.GetServerResponse();
        return TypedResults.Ok(response);
    }
    catch (Exception e)
    {
        return TypedResults.Problem(detail:e.Message, statusCode: 500, title: "Server error");
    }
}