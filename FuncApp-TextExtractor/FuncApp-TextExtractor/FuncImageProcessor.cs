using Azure.Messaging.ServiceBus;
using FuncApp.TextExtractor.Configuration;
using FuncApp.TextExtractor.Data.Dtos;
using FuncApp.TextExtractor.ImagesStorage;
using FuncApp.TextExtractor.OCR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace FuncApp.TextExtractor;

public class FuncImageProcessor(ILogger<FuncImageProcessor> logger, IOptions<FunctionSettings> settings, IImagesStorageService imagesBlobStorageService, IOCRService ocrService)
{
    private readonly ILogger<FuncImageProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly FunctionSettings _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
    private readonly IImagesStorageService _imagesBlobStorageService = imagesBlobStorageService ?? throw new ArgumentNullException(nameof(imagesBlobStorageService));
    private readonly IOCRService _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));

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

            // Use the OCR service to perform OCR on the image
            var ocrResult = _ocrService.ExtractTextFromImageAsync($"{imageProcessingMessage.StorageLocation}{imageProcessingMessage.ImageName}").Result;

            // Process the OCR result as needed
            _logger.LogInformation($"Extracted Text: {ocrResult}");

            // Call method to move image to processed container // Use Wait() since Azure Functions does not support async main
            _imagesBlobStorageService.MoveImageToProcessedContainerAsync(imageProcessingMessage.ImageName).Wait();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing message: {ex.Message}");
            throw;
        }
    }

}
