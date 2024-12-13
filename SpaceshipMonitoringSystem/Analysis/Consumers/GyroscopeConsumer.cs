using Analysis.DTO;
using Analysis.Entities;
using Analysis.Services;
using Analysis.Producers;
using Analysis.Repositories;
using MassTransit;

namespace Analysis.Consumers;

public class GyroscopeConsumer : IConsumer<GyroscopeMessage>
{
    private readonly ILogger<GyroscopeConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;
    private readonly ISensorsRepository<Gyroscope> _sensorsRepository;

    public GyroscopeConsumer(ILogger<GyroscopeConsumer> logger, IAnalysisService analysisService, 
    IBasicProducer<AlertMessage> analysisProducer, ISensorsRepository<Gyroscope> sensorsRepository)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
        _sensorsRepository = sensorsRepository;
    }

    public async Task Consume(ConsumeContext<GyroscopeMessage> context)
    {
        _logger.LogInformation
        (
            "[{Timestamp}] Gyroscope message X: {X} °/s, Y:{Y} °/s, Z: {Z} °/s", 
            context.Message.Timestamp,
            context.Message.X, 
            context.Message.Y, 
            context.Message.Z
        );

         var gyroscope = new Gyroscope
        {
            Timestamp = context.Message.Timestamp.ToString("o"),
            Name = "Gyroscope Sensor",
            X = context.Message.X,
            Y = context.Message.Y,
            Z = context.Message.Z
        };

        try
        {
            await _sensorsRepository.Create(gyroscope);
            _logger.LogInformation("Gyroscope data saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save gyroscope data");
        }
        
        // Os 3 eixos são independentes
        var axes = new Dictionary<string, double>
        {
            { "X", context.Message.X },
            { "Y", context.Message.Y },
            { "Z", context.Message.Z }
        };

        foreach (var (axis, value) in axes)
        {
            var isValueNormal = _analysisService.IsValueNormal(value, -5.0, 5.0);
            
            if (!isValueNormal)
            {
                _logger.LogWarning("Anomaly detected: Gyroscope's axis {Axis} value {Value} °/s is out of range", axis, value);

                var alertMessage = new AlertMessage
                {
                    Type = "Gyroscope",
                    Message = $"Anomaly detected: Axis {axis} value {value} °/s is out of range."
                };

                try
                {
                    await _analysisProducer.PublishAsync(alertMessage);
                    _logger.LogInformation("Alert message sent successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send alert message");
                }
            }
        }
    }
}