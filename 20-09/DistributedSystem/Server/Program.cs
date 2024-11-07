using System.Text.Json;
using Common;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Server.Extensions;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.ConfigureLogging();
builder.Services.AddServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var sampleService = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();
        await sampleService.SendHi();
    }
    catch (Exception e)
    {
        Log.Information(e,"Failed to register service!");
    }
}

app.MapHealthChecks("_health");

app.MapGet("/", () => TypedResults.Ok())
    .WithName("Home")
    .WithOpenApi();

app.MapPost("/receive", ([FromBody] SumRequest request) =>
    {
        var response = new SumResponse(request.A + request.B);
        return TypedResults.Ok(response);
    })
    .WithName("Receive")
    .WithOpenApi();


app.Run();