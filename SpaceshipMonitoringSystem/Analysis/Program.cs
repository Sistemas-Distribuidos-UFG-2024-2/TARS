using Analysis.Extensions;
using Analysis.Utils;
using Analysis.Services;
using Serilog;

SerilogConfiguration.ConfigureLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSerilog();
// builder.Services.AddServices();
builder.Services.AddRabbitMqService(builder.Configuration);
builder.Services.AddScoped<IAnalysisService, AnalysisService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.MapGet("/", () => Results.Ok("Analytics up and running..."));

app.Run();
