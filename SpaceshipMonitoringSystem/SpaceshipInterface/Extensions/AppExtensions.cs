using MassTransit;
using MassTransit.RabbitMqTransport;
using SpaceshipInterface.Consumers;
using SpaceshipInterface.Producers;

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
            
            configurator.UsingRabbitMq((context, factoryConfigurator) =>
            {
                factoryConfigurator.Host(new Uri(rabbitMqHost), host =>
                {
                    host.Username(username);
                    host.Password(password);
                });

                factoryConfigurator.UseRawJsonSerializer();
                factoryConfigurator.UseRawJsonDeserializer();
                
                factoryConfigurator.ReceiveEndpoint("houston", endpoint =>
                {
                    endpoint.ConfigureConsumer<HoustonConsumer>(context);
                });
                
                factoryConfigurator.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<HoustonProducer>();
    }
}