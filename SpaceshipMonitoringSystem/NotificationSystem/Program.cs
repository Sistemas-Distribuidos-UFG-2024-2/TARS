using System.Text.Json;
using MongoDB.Bson;
using NotificationSystem.DTO;
using NotificationSystem.Extensions;
using NotificationSystem.Services;
using NotificationSystem.Utils;
using Serilog;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

SerilogConfiguration.ConfigureLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSerilog();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new ObjectIdConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddAuthorization();
builder.Services.AddMongoDb();
builder.Services.AddServices();
builder.Services.AddRabbitMqService(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.MapPost("api/persons", async (PersonCreateDto personCreateDto, IPersonsService service) =>
{
    var id = await service.Create(personCreateDto);
    return Results.Created($"api/persons/{id.ToString()}", personCreateDto);
});

app.MapGet("api/persons/{id}", async (IPersonsService service, string id) =>
{
    var person = await service.GetById(ObjectId.Parse(id));
    return Results.Ok(person);
});

app.MapGet("api/persons", async (IPersonsService service) =>
{
    var persons = await service.GetAll();
    return Results.Ok(persons);
});

app.MapPatch("api/persons/{id}", async (string id, PersonUpdateDto personUpdateDto, IPersonsService service) =>
{
    var person = await service.Update(id, personUpdateDto);
    return Results.Ok(person);
});

app.MapDelete("api/persons/{id}", async (string id, IPersonsService service) =>
{
    var deleted = await service.Delete(id);
    return Results.Ok(deleted);
});

app.MapGet("/", () => Results.Ok("Notification system up and running..."));

app.Run();
