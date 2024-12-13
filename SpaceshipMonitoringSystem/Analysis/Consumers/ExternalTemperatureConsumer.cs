using Analysis.DTO;
using Analysis.Entities;
using Analysis.Services;
using Analysis.Producers;
using Analysis.Repositories;
using MassTransit;

namespace Analysis.Consumers;

public class ExternalTemperatureConsumer : IConsumer<ExternalTemperatureMessage>
{
    private readonly ILogger<ExternalTemperatureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;
    private readonly ISensorsRepository<ExternalTemperature> _sensorsRepository;

    public ExternalTemperatureConsumer(ILogger<ExternalTemperatureConsumer> logger, IAnalysisService analysisService, 
        IBasicProducer<AlertMessage> analysisProducer, ISensorsRepository<ExternalTemperature> sensorsRepository)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
        _sensorsRepository = sensorsRepository;
    }

    public async Task Consume(ConsumeContext<ExternalTemperatureMessage> context)
    {
        _logger.LogInformation("[{Timestamp}] External temperature: {Temp} ºC", context.Message.Timestamp, context.Message.Externaltemperature);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.Externaltemperature, -150.0, 150.0);

        var externalTemperature = new ExternalTemperature
        {
            Timestamp = context.Message.Timestamp.ToString("o"),
            Name = "External Temperature Sensor",
            Value = context.Message.Externaltemperature
        };

        try
        {
            await _sensorsRepository.Create(externalTemperature);
            _logger.LogInformation("External temperature data saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save external temperature data");
        }

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: External temperature value {Temp} ºC is out of range", context.Message.Externaltemperature);

            var alertMessage = new AlertMessage
            {
                Type = "External Temperature",
                Message = $"Anomaly detected: Value {context.Message.Externaltemperature} ºC is out of range."
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