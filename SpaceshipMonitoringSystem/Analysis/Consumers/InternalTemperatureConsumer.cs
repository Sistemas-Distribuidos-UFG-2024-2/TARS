using Analysis.DTO;
using Analysis.Entities;
using Analysis.Services;
using Analysis.Producers;
using Analysis.Repositories;
using MassTransit;

namespace Analysis.Consumers;

public class InternalTemperatureConsumer : IConsumer<InternalTemperatureMessage>
{
    private readonly ILogger<InternalTemperatureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;
    private readonly ISensorsRepository<InternalTemperature> _sensorsRepository;

    public InternalTemperatureConsumer(ILogger<InternalTemperatureConsumer> logger, IAnalysisService analysisService, 
        IBasicProducer<AlertMessage> analysisProducer, ISensorsRepository<InternalTemperature> sensorsRepository)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
        _sensorsRepository = sensorsRepository;
    }

    public async Task Consume(ConsumeContext<InternalTemperatureMessage> context)
    {
        _logger.LogInformation("[{Timestamp}] Internal temperature: {Temp} ºC", context.Message.Timestamp, context.Message.InternalTemperature);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.InternalTemperature, 12.0, 25.0);

        var internalTemperature = new InternalTemperature
        {
            Timestamp = context.Message.Timestamp.ToString("o"),
            Name = "Internal Temperature Sensor",
            Value = context.Message.InternalTemperature
        };

        try
        {
            await _sensorsRepository.Create(internalTemperature);
            _logger.LogInformation("Internal temperature data saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save internal temperature data");
        }

        if (!isValueNormal) 
        {
            _logger.LogWarning("Anomaly detected: Internal temperature value {Temp} ºC is out of range", context.Message.InternalTemperature );

            var alertMessage = new AlertMessage
            {
                Type = "Internal Temperature",
                Message = $"Anomaly detected: Value {context.Message.InternalTemperature} ºC is out of range."
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