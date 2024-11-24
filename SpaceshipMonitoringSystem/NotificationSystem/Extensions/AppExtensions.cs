using MassTransit;
using MassTransit.RabbitMqTransport;
using NotificationSystem.Consumers;
using NotificationSystem.Producers;
using NotificationSystem.DTO;

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

                // // Exchange para alert message
                // factoryConfigurator.Message<AlertMessage>(configuration =>
                // {
                //     configuration.SetEntityName("alert-exchange");
                // });

                // factoryConfigurator.UseRawJsonDeserializer();

                factoryConfigurator.ReceiveEndpoint("notification-alert-queue", endpoint =>
                {
                    endpoint.ConfigureConsumer<AnalysisConsumer>(context);

                    // Vincula a fila à exchange
                    endpoint.Bind("alert-exchange", x =>
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