using NotificationSystem.Consumers;
using NotificationSystem.Producers;
using MassTransit;
using NotificationSystem.Database;
using NotificationSystem.Repositories;
using NotificationSystem.Services;

namespace NotificationSystem.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddMongoDb(this IServiceCollection services)
    {
        services.AddSingleton<IMongoDbConfig, MongoDbConfig>();
        services.AddSingleton<IMongoDbConnection, MongoDbConnection>();
        services.AddSingleton<IMongoDbContext, MongoDbContext>();
    }

    public static void AddServices(this IServiceCollection services)
    {
        
        services.AddSingleton<IPersonsRepository, PersonsRepository>();
        services.AddSingleton<IPersonsService, PersonsService>();

        services.AddSingleton<IMailService, MailService>();
    }

    public static void AddRabbitMqService(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("RabbitMQ");

        var rabbitMqHost = section.GetValue<string>("Host")!;
        var username = section.GetValue<string>("Username")!;
        var password = section.GetValue<string>("Password")!;

        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<AnalysisConsumer>();

            configurator.UsingRabbitMq((context, factoryConfigurator) =>
            {
                factoryConfigurator.Host(new Uri(rabbitMqHost), host =>
                {
                    host.Username(username);
                    host.Password(password);
                });

                factoryConfigurator.UseRawJsonSerializer();
                factoryConfigurator.UseRawJsonDeserializer();

                // Configura a fila para consumir dados da exchange de alerta
                factoryConfigurator.ReceiveEndpoint("notification-alerts-queue", endpoint =>
                {
                    endpoint.ConfigureConsumer<AnalysisConsumer>(context);
                    endpoint.Bind("alerts-exchange", x => { x.ExchangeType = "fanout"; });
                });

                factoryConfigurator.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<BasicProducer>();
    }
}