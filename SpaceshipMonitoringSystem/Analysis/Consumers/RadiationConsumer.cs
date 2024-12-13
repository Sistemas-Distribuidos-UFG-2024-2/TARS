using Analysis.DTO;
using Analysis.Entities;
using Analysis.Services;
using Analysis.Producers;
using Analysis.Repositories;
using MassTransit;

namespace Analysis.Consumers;

public class RadiationConsumer : IConsumer<RadiationMessage>
{
    private readonly ILogger<RadiationConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;
    private readonly ISensorsRepository<Radiation> _sensorsRepository;

    public RadiationConsumer(ILogger<RadiationConsumer> logger, IAnalysisService analysisService, 
        IBasicProducer<AlertMessage> analysisProducer, ISensorsRepository<Radiation> sensorsRepository)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
        _sensorsRepository = sensorsRepository;
    }

    public async Task Consume(ConsumeContext<RadiationMessage> context)
    {
        _logger.LogInformation("[{Timestamp}] Radiation: {Rad} μSv/h", context.Message.Timestamp, context.Message.Radiation);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Radiation, 50.0, 500.0);

        var radiation = new Radiation
        {
            Timestamp = context.Message.Timestamp.ToString("o"),
            Name = "Radiation Sensor",
            Value = context.Message.Radiation
        };

        try
        {
            await _sensorsRepository.Create(radiation);
            _logger.LogInformation("Radiation data saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save radiation data");
        }

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Radiation value {Rad} μSv/h is out of range", context.Message.Radiation);

            var alertMessage = new AlertMessage
            {
                Type = "Radiation",
                Message = $"Anomaly detected: Value {context.Message.Radiation} μSv/h is out of range."
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