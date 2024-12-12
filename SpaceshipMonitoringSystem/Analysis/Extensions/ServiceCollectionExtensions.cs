using Analysis.Consumers;
using Analysis.Database;
using Analysis.Producers;
using Analysis.Services;
using Analysis.DTO;
using Analysis.Entities;
using Analysis.Repositories;
using MassTransit;

namespace Analysis.Extensions;

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
        services.AddScoped<IAnalysisService, AnalysisService>();
        services.AddSingleton<ISensorsRepository<Acceleration>, SensorsRepository<Acceleration>>();
        services.AddSingleton<ISensorsRepository<ExternalTemperature>, SensorsRepository<ExternalTemperature>>();
        services.AddSingleton<ISensorsRepository<FuelPressure>, SensorsRepository<FuelPressure>>();
        services.AddSingleton<ISensorsRepository<Gyroscope>, SensorsRepository<Gyroscope>>();
        services.AddSingleton<ISensorsRepository<InternalPressure>, SensorsRepository<InternalPressure>>();
        services.AddSingleton<ISensorsRepository<InternalTemperature>, SensorsRepository<InternalTemperature>>();
        services.AddSingleton<ISensorsRepository<Radiation>, SensorsRepository<Radiation>>();
    }

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

                factoryConfigurator.UseRawJsonSerializer();
                factoryConfigurator.UseRawJsonDeserializer();

                // Exchange para a publicação das mensagens de alerta
                factoryConfigurator.Message<AlertMessage>(messageTopologyConfigurator =>
                {
                    messageTopologyConfigurator.SetEntityName("alerts-exchange");
                });

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

        services.AddScoped<IBasicProducer<AlertMessage>, AnalysisProducer>();
    }
}