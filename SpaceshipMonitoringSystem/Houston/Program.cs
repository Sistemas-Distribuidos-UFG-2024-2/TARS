using System.Text.Json;
using Houston.DTO;
using Houston.Entities;
using Houston.Extensions;
using Houston.Producers;
using Houston.Services;
using Houston.Utils;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

SerilogConfiguration.ConfigureLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new ObjectIdConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSerilog();
builder.Services.AddMongoDb();
builder.Services.AddRabbitMqService(builder.Configuration);
builder.Services.AddServices();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.UseSerilogRequestLogging();
app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem()
            .ExecuteAsync(context)));


// Usando 'string text' o texto é enviado na URL da requisição como uma query, e não aceita o envio da mensagem no body da requisição como um json
// Exemplo: http://localhost:5008/api/spaceship?text="Olá humano"
app.MapPost("api/spaceship", async (SpaceshipMessage message, SpaceshipProducer producer) =>
{
    await producer.PublishAsync(message);
    return Results.Ok("Message published");
});

app.MapGet("api/sensors/acceleration",
    async ([FromServices] ISensorsService service, int limit = 10, int offset = 0) =>
    {
        var list = await service.GetAll<Acceleration>(limit, offset);
        return Results.Ok(list.Select(x => new SensorData(x.Value, x.Timestamp)));
    });

app.MapGet("api/sensors/external-temperature",
    async ([FromServices] ISensorsService service, int limit = 10, int offset = 0) =>
    {
        var list = await service.GetAll<ExternalTemperature>(limit, offset);
        return Results.Ok(list.Select(x => new SensorData(x.Value, x.Timestamp)));
    });

app.MapGet("api/sensors/fuel-pressure",
    async ([FromServices] ISensorsService service, int limit = 10, int offset = 0) =>
    {
        var list = await service.GetAll<FuelPressure>(limit, offset);
        return Results.Ok(list.Select(x => new SensorData(x.Value, x.Timestamp)));
    });

app.MapGet("api/sensors/internal-pressure",
    async ([FromServices] ISensorsService service, int limit = 10, int offset = 0) =>
    {
        var list = await service.GetAll<InternalPressure>(limit, offset);
        return Results.Ok(list.Select(x => new SensorData(x.Value, x.Timestamp)));
    });

app.MapGet("api/sensors/internal-temperature",
    async ([FromServices] ISensorsService service, int limit = 10, int offset = 0) =>
    {
        var list = await service.GetAll<InternalTemperature>(limit, offset);
        return Results.Ok(list.Select(x => new SensorData(x.Value, x.Timestamp)));
    });

app.MapGet("api/sensors/radiation",
    async ([FromServices] ISensorsService service, int limit = 10, int offset = 0) =>
    {
        var list = await service.GetAll<Radiation>(limit, offset);
        return Results.Ok(list.Select(x => new SensorData(x.Value, x.Timestamp)));
    });

app.MapGet("api/sensors/gyroscope",
    async ([FromServices] ISensorsService service, int limit = 10, int offset = 0) =>
    {
        var list = await service.GetAll<Gyroscope>(limit, offset);
        return Results.Ok(list.Select(g => new { g.X, g.Y, g.Z, g.Timestamp }));
    });

app.MapGet("/", () => Results.Ok("Houston up and running..."));

app.Run();