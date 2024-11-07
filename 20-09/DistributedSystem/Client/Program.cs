using Client.Extensions;
using Client.Services;
using Common;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureLogging();
builder.Services.AddServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => TypedResults.Ok("Client is up and running!"))
    .WithName("Home")
    .WithOpenApi();


app.MapPost("/send", GetServerResponse)
    .WithName("Send")
    .WithOpenApi();

app.Run();

return;

async Task<IResult> GetServerResponse([FromKeyedServices("server")] IServerService serverService, [FromBody] SumRequest request){
    try
    {
        var response = await serverService.GetServerResponse(request);
        return TypedResults.Ok(response);
    }
    catch (Exception e)
    {
        return TypedResults.Problem(detail:e.Message, statusCode: 500, title: "Server error");
    }
}