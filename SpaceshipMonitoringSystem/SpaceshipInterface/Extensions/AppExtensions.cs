using MassTransit;
using MassTransit.RabbitMqTransport;
using SpaceshipInterface.Consumers;
using SpaceshipInterface.Producers;
using SpaceshipInterface.DTO;

namespace SpaceshipInterface.Extensions;

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
            configurator.AddConsumer<HoustonConsumer>();
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
                factoryConfigurator.ReceiveEndpoint("spaceship-alerts-queue", endpoint =>
                {
                    endpoint.ConfigureConsumer<AnalysisConsumer>(context);
                    endpoint.Bind("alerts-exchange", x =>
                    {
                        x.ExchangeType = "fanout";
                    });
                });
                
                factoryConfigurator.ReceiveEndpoint("spaceship", endpoint =>
                {
                    endpoint.ConfigureConsumer<HoustonConsumer>(context);
                });
                
                factoryConfigurator.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<HoustonProducer>();
    }
}