using Houston.Consumers;
using Houston.Producers;
using Commom.DTO;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace Houston.Extensions;

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
            configurator.AddConsumer<SpaceshipConsumer>();
            configurator.AddConsumer<AnalysisConsumer>();
            
            configurator.UsingRabbitMq((context, factoryConfigurator) =>
            {
                factoryConfigurator.Host(new Uri(rabbitMqHost), host =>
                {
                    host.Username(username);
                    host.Password(password);
                });

                // Exchange para alert message
                // factoryConfigurator.Message<AlertMessage>(configuration =>
                // {
                //     configuration.SetEntityName("alert-exchange");
                // });

                // factoryConfigurator.UseRawJsonDeserializer();

                factoryConfigurator.ReceiveEndpoint("houston-alert-queue", endpoint =>
                {
                    endpoint.ConfigureConsumer<AnalysisConsumer>(context);

                    // Vincula a fila à exchange
                    endpoint.Bind("alert-exchange", x =>
                    {
                        x.ExchangeType = "fanout";
                    });
                });

                factoryConfigurator.ReceiveEndpoint("spaceship", endpoint =>
                {
                    endpoint.ConfigureConsumer<SpaceshipConsumer>(context);
                });
                
                factoryConfigurator.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<SpaceshipProducer>();
    }
}