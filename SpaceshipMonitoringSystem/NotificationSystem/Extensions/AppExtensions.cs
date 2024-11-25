using NotificationSystem.Consumers;
using NotificationSystem.Producers;
using NotificationSystem.DTO;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace NotificationSystem.Extensions;

public static class AppExtensions
{
    public static void AddRabbitMqService(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("RabbitMQ");
        
        var rabbitMqHost = section.GetValue<string>("Host")!;
        var username = section.GetValue<string>("Username")!;
        var password = section.GetValue<string>("Password")!;

        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<BasicConsumer>();
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
                    endpoint.Bind("alerts-exchange", x =>
                    {
                        x.ExchangeType = "fanout";
                    });
                });

                factoryConfigurator.ReceiveEndpoint("basic-queue", endpoint =>
                {
                    endpoint.ConfigureConsumer<BasicConsumer>(context);
                });
                
                factoryConfigurator.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<BasicProducer>();
    }
}