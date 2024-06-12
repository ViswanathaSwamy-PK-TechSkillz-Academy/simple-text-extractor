using Azure.Messaging.ServiceBus;
using FuncApp_TextExtractor.Configuration;
using FuncApp_TextExtractor.Data.Dtos;
using FuncApp_TextExtractor.ImagesBlobStorage;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace FuncApp_TextExtractor;

public class FuncImageProcessor(ILogger<FuncImageProcessor> logger, IOptions<FunctionSettings> settings, IImagesStorageService imagesBlobStorageService)
{
    private readonly ILogger<FuncImageProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly FunctionSettings _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
    private readonly IImagesStorageService _imagesBlobStorageService = imagesBlobStorageService ?? throw new ArgumentNullException(nameof(imagesBlobStorageService));

    [Function(nameof(FuncImageProcessor))]
    public void Run([ServiceBusTrigger("image-processing-queue", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
    {
        try
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            string messageBody = Encoding.UTF8.GetString(message.Body);
            var imageProcessingMessage = JsonConvert.DeserializeObject<ImageProcessingMessageDto>(messageBody);

            _logger.LogInformation($"Processing image: {imageProcessingMessage.ImageName} :: {imageProcessingMessage.Language} :: {imageProcessingMessage.StorageLocation}");

            // Call method to move image to processed container
            _imagesBlobStorageService.MoveImageToProcessedContainerAsync(imageProcessingMessage.ImageName).Wait(); // Use Wait() since Azure Functions does not support async main
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing message: {ex.Message}");
            throw;
        }
    }

}
