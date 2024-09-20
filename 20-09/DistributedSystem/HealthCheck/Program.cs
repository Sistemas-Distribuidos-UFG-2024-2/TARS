using HealthCheck.Extensions;
using HealthCheck.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureLogging();
builder.Services.AddServices(builder.Configuration);


var app = builder.Build();

app.MapPost("hi", async (HttpContext httpContext, IRedisService redisService) =>
{
    var origin = httpContext.Request.Headers.Origin.ToArray();
    
    if (origin.Length > 0)
    {
        await redisService.AddSetItems("registered_servers", origin!);
    }

    return TypedResults.Ok();
});

app.MapGet("get-healthy-servers", async (IRedisService redisService) =>
{
    var healthyServers = await redisService.GetSet("servers");

    return TypedResults.Ok(healthyServers);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();