using AnalysisService.DTO;
using AnalysisService.Extensions;
using AnalysisService.Producers;
using AnalysisService.Utils;
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

app.MapGet("/", () => Results.Ok("Analytics up and running..."));

app.Run();
