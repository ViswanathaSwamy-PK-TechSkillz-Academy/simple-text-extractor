using Azure.Messaging.ServiceBus;
using FuncApp_TextExtractor.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FuncApp_TextExtractor;

public class FuncImageProcessor
{
    private readonly ILogger<FuncImageProcessor> _logger;
    private readonly FunctionSettings _settings;

    public FuncImageProcessor(ILogger<FuncImageProcessor> logger, FunctionSettings settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    [Function(nameof(FuncImageProcessor))]
    public void Run([ServiceBusTrigger("image-processing-queue", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
    }
}
