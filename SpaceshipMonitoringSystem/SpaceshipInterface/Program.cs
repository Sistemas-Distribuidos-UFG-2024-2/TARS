using Serilog;
using SpaceshipInterface.DTO;
using SpaceshipInterface.Extensions;
using SpaceshipInterface.Producers;
using SpaceshipInterface.Utils;
using SpaceshipInterface.Sockets;

SerilogConfiguration.ConfigureLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSerilog();
builder.Services.AddRabbitMqService(builder.Configuration);
builder.Services.AddSingleton<SensorServer>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<SensorServer>>();
    return new SensorServer(5101, logger);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.MapPost("api/houston", async (HoustonMessage message, HoustonProducer producer) =>
{
    await producer.PublishAsync(message);
    return Results.Ok("Message published");
});

app.MapGet("/", () => Results.Ok("Spaceship interface up and running..."));

var sensorServer = app.Services.GetRequiredService<SensorServer>();
_ = sensorServer.StartAsync();

app.Run();
