using MassTransit;
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
            configurator.AddConsumer<BasicConsumer>();
            
            configurator.UsingRabbitMq((context, factoryConfigurator) =>
            {
                factoryConfigurator.Host(new Uri(rabbitMqHost), host =>
                {
                    host.Username(username);
                    host.Password(password);
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