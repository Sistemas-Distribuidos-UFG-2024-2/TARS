using Houston.DTO;
using Houston.Extensions;
using Houston.Producers;
using Houston.Utils;
using Serilog;

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

// Usando 'string text' o texto é enviado na URL da requisição como uma query, e não aceita o envio da mensagem no body da requisição como um json
// Exemplo: http://localhost:5008/api/spaceship?text="Olá humano"
app.MapPost("api/spaceship", async (SpaceshipMessage message, SpaceshipProducer producer) =>
{
   await producer.PublishAsync(message);
   return Results.Ok("Message published");
});

app.MapGet("/", () => Results.Ok("Houston up and running..."));

app.Run();
