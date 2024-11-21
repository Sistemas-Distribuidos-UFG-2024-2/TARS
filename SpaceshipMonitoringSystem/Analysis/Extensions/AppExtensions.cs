using Analysis.Consumers;
using Analysis.Producers;
using MassTransit;

namespace Analysis.Extensions;

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
            configurator.AddConsumer<AccelerationConsumer>();
            configurator.AddConsumer<ExternalTemperatureConsumer>();
            configurator.AddConsumer<FuelPressureConsumer>();
            configurator.AddConsumer<GyroscopeConsumer>();
            configurator.AddConsumer<InternalPressureConsumer>();
            configurator.AddConsumer<InternalTemperatureConsumer>();
            configurator.AddConsumer<RadiationConsumer>();

            configurator.UsingRabbitMq((context, factoryConfigurator) =>
            {
                factoryConfigurator.Host(new Uri(rabbitMqHost), host =>
                {
                    host.Username(username);
                    host.Password(password);
                });

                factoryConfigurator.UseRawJsonDeserializer();

                factoryConfigurator.ReceiveEndpoint("acceleration_queue",
                    endpoint => { endpoint.ConfigureConsumer<AccelerationConsumer>(context); });

                factoryConfigurator.ReceiveEndpoint("external_temperature_queue",
                    endpoint => { endpoint.ConfigureConsumer<ExternalTemperatureConsumer>(context); });

                factoryConfigurator.ReceiveEndpoint("fuel_pressure_queue",
                    endpoint => { endpoint.ConfigureConsumer<FuelPressureConsumer>(context); });

                factoryConfigurator.ReceiveEndpoint("gyroscope_queue",
                    endpoint => { endpoint.ConfigureConsumer<GyroscopeConsumer>(context); });

                factoryConfigurator.ReceiveEndpoint("internal_pressure_queue",
                    endpoint => { endpoint.ConfigureConsumer<InternalPressureConsumer>(context); });

                factoryConfigurator.ReceiveEndpoint("internal_temperature_queue",
                    endpoint => { endpoint.ConfigureConsumer<InternalTemperatureConsumer>(context); });

                factoryConfigurator.ReceiveEndpoint("radiation_queue",
                    endpoint => { endpoint.ConfigureConsumer<RadiationConsumer>(context); });

                factoryConfigurator.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<BasicProducer>();
    }
}