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

app.MapPost("api/spaceship", async (string text, SpaceshipProducer producer) =>
{
   await producer.PublishAsync(new SpaceshipMessage(text));
   return Results.Ok("Message published");
});

app.MapGet("/", () => Results.Ok("Houston up and running..."));

app.Run();
