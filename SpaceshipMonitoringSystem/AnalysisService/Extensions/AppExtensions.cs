using AnalysisService.Consumers;
using AnalysisService.Producers;
using MassTransit;

namespace AnalysisService.Extensions;

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
            // configurator.AddConsumer<ExternalTemperatureConsumer>();
            configurator.AddConsumer<GyroscopeConsumer>();
            configurator.AddConsumer<AccelerationConsumer>();
            
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
                
                /*factoryConfigurator.ReceiveEndpoint("external_temperature_queue", endpoint =>
                {
                    endpoint.ConfigureConsumer<ExternalTemperatureConsumer>(context);
                });*/
                
                factoryConfigurator.ReceiveEndpoint("gyroscope_queue", endpoint =>
                {
                    endpoint.UseRawJsonSerializer();
                    endpoint.UseRawJsonDeserializer();
                    endpoint.ConfigureConsumer<GyroscopeConsumer>(context);
                });
                
                factoryConfigurator.ReceiveEndpoint("acceleration_queue", endpoint =>
                {
                    endpoint.UseRawJsonSerializer();
                    endpoint.UseRawJsonDeserializer();
                    endpoint.ConfigureConsumer<AccelerationConsumer>(context);
                });
                
                factoryConfigurator.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<BasicProducer>();
    }
}