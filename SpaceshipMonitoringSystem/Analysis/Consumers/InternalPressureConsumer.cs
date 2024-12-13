using Analysis.DTO;
using Analysis.Entities;
using Analysis.Services;
using Analysis.Producers;
using Analysis.Repositories;
using MassTransit;

namespace Analysis.Consumers;

public class InternalPressureConsumer : IConsumer<InternalPressureMessage>
{
    private readonly ILogger<InternalPressureConsumer> _logger;
    private readonly IAnalysisService _analysisService;
    private readonly IBasicProducer<AlertMessage> _analysisProducer;
    private readonly ISensorsRepository<InternalPressure> _sensorsRepository;

    public InternalPressureConsumer(ILogger<InternalPressureConsumer> logger, IAnalysisService analysisService, 
        IBasicProducer<AlertMessage> analysisProducer, ISensorsRepository<InternalPressure> sensorsRepository)
    {
        _logger = logger;
        _analysisService = analysisService;
        _analysisProducer = analysisProducer;
        _sensorsRepository = sensorsRepository;
    }

    public async Task Consume(ConsumeContext<InternalPressureMessage> context)
    {
        _logger.LogInformation("[{Timestamp}] Internal pressure: {Pressure} kPa", context.Message.Timestamp, context.Message.InternalPressure);

        var isValueNormal = _analysisService.IsValueNormal(context.Message.InternalPressure, 100.0, 103.0);

        var internalPressure = new InternalPressure
        {
            Timestamp = context.Message.Timestamp.ToString("o"),
            Name = "Internal Pressure Sensor",
            Value = context.Message.InternalPressure
        };

        try
        {
            await _sensorsRepository.Create(internalPressure);
            _logger.LogInformation("Internal pressure data saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save internal pressure data");
        }

        if (!isValueNormal)
        {
            _logger.LogWarning("Anomaly detected: Internal pressure value {Pressure} kPa is out of range", context.Message.InternalPressure);

            var alertMessage = new AlertMessage
            {
                Type = "Internal Pressure",
                Message = $"Anomaly detected: Value {context.Message.InternalPressure} kPa is out of range."
            };

            // Produz uma mensagem de alerta para o serviço de notificação e para o Houston
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