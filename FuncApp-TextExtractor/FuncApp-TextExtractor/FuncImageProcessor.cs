using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FuncApp_TextExtractor
{
    public class FuncImageProcessor
    {
        private readonly ILogger<FuncImageProcessor> _logger;

        public FuncImageProcessor(ILogger<FuncImageProcessor> logger)
        {
            _logger = logger;
        }

        [Function(nameof(FuncImageProcessor))]
        public void Run([ServiceBusTrigger("image-processing-queue", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        }
    }
}
