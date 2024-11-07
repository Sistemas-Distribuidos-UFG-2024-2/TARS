using NotificationSystem.DTO;
using NotificationSystem.Extensions;
using NotificationSystem.Producers;
using NotificationSystem.Utils;
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

app.MapPost("state/{state}", async (string state, BasicProducer producer) =>
{
   await producer.PublishAsync(new BasicMessage { State = state });
});

app.Run();
