using Serilog;
using SpaceshipInterface.DTO;
using SpaceshipInterface.Extensions;
using SpaceshipInterface.Producers;
using SpaceshipInterface.Utils;

SerilogConfiguration.ConfigureLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSerilog();
builder.Services.AddRabbitMqService(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.MapPost("api/houston", async (string text, HoustonProducer producer) =>
{
    await producer.PublishAsync(new HoustonMessage(text));
    return Results.Ok("Message published");
});

app.MapGet("/", () => Results.Ok("Spaceship interface up and running..."));

app.Run();
