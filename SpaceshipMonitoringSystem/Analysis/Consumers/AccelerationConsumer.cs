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
        //Sensor acceleration = new Sensor (context.Message.timeStamp,context.Message.name, context.Message.Acceleration)
        //acceleration. To totalmente perdido nessa parte, eu sei que eu preciso de alguma forma chamar a função de salvar 
        //no BD e passar o próprio sensor como parâmetro, 
        //o problema é como puxar o sensor obj pra passar, eu crio ele novamente? to meio perdido ainda, tenh0 que analisar melhor
        //a logica do codigo
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