using Analysis.DTO;
using Analysis.Entities;
using Analysis.Services;
using Analysis.Producers;
using Analysis.Repositories;
using MassTransit;

namespace Analysis.Consumers;

public class AccelerationConsumer : IConsumer<AccelerationMessage>
{
    private readonly ILogger<AccelerationConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;
    private readonly ISensorsRepository<Acceleration> _sensorsRepository;

    public AccelerationConsumer(ILogger<AccelerationConsumer> logger, IAnalysisService analysisService,
        IBasicProducer<AlertMessage> analysisProducer, ISensorsRepository<Acceleration> sensorsRepository)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
        _sensorsRepository = sensorsRepository;
    }

    public async Task Consume(ConsumeContext<AccelerationMessage> context)
    {
        _logger.LogInformation("Acceleration: {Acceleration} µm", context.Message.Acceleration);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Acceleration, -1.0, 1.0);
        
        var acceleration = new Acceleration
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            Name = "Acceleration Sensor",
            Value = context.Message.Acceleration
        };

        try
        {
            await _sensorsRepository.Create(acceleration);
            _logger.LogInformation("Acceleration data saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save acceleration data");
        }

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Acceleration value {Acceleration} µm is out of range",
                context.Message.Acceleration);

            var alertMessage = new AlertMessage
            {
                Type = "Acceleration",
                Message = $"Anomaly detected: Value {context.Message.Acceleration} µm is out of range."
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