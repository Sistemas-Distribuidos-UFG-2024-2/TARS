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

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseSerilogRequestLogging();

app.MapGet("state/{state}", async (string state, BasicProducer producer) =>
{
   await producer.PublishAsync(new BasicMessage { State = state });
});

app.MapGet("/", () => Results.Ok("Houston up and running..."));

app.Run();
