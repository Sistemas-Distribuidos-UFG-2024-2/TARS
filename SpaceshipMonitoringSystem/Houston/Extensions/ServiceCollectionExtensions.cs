using Houston.Consumers;
using Houston.Database;
using Houston.Entities;
using Houston.Producers;
using Houston.Repositories;
using Houston.Services;
using MassTransit;

namespace Houston.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<ISensorsRepository<Acceleration>, SensorsRepository<Acceleration>>();
        services.AddSingleton<ISensorsRepository<ExternalTemperature>, SensorsRepository<ExternalTemperature>>();
        services.AddSingleton<ISensorsRepository<FuelPressure>, SensorsRepository<FuelPressure>>();
        services.AddSingleton<ISensorsRepository<Gyroscope>, SensorsRepository<Gyroscope>>();
        services.AddSingleton<ISensorsRepository<InternalPressure>, SensorsRepository<InternalPressure>>();
        services.AddSingleton<ISensorsRepository<InternalTemperature>, SensorsRepository<InternalTemperature>>();
        services.AddSingleton<ISensorsRepository<Radiation>, SensorsRepository<Radiation>>();
        services.AddScoped<ISensorsService, SensorsService>();

    }
    public static void AddMongoDb(this IServiceCollection services)
    {
        services.AddSingleton<IMongoDbConfig, MongoDbConfig>();
        services.AddSingleton<IMongoDbConnection, MongoDbConnection>();
        services.AddSingleton<IMongoDbContext, MongoDbContext>();
    }
    
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

                factoryConfigurator.UseRawJsonSerializer();
                factoryConfigurator.UseRawJsonDeserializer();

                // Configura a fila para consumir dados da exchange de alerta
                factoryConfigurator.ReceiveEndpoint("houston-alerts-queue", endpoint => 
                {
                    endpoint.ConfigureConsumer<AnalysisConsumer>(context);
                    endpoint.Bind("alerts-exchange", x =>
                    {
                        x.ExchangeType = "fanout";
                    });
                });

                factoryConfigurator.ReceiveEndpoint("houston", endpoint =>
                {
                    endpoint.ConfigureConsumer<SpaceshipConsumer>(context);
                });
                
                factoryConfigurator.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<SpaceshipProducer>();
    }
}