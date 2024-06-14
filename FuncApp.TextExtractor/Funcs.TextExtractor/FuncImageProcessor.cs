using Azure.Messaging.ServiceBus;
using Funcs.TextExtractor.Configuration;
using Funcs.TextExtractor.Data.Dtos;
using Funcs.TextExtractor.Data.Entities;
using Funcs.TextExtractor.Data.Repositories;
using Funcs.TextExtractor.ImagesStorage;
using Funcs.TextExtractor.OCR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Funcs.TextExtractor;

public class FuncImageProcessor(ILogger<FuncImageProcessor> logger, IOptions<FunctionSettings> settings, IImagesStorageService imagesBlobStorageService, IOCRService ocrService, IImageProcessingTaskRepository taskRepository)
{
    private readonly ILogger<FuncImageProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly FunctionSettings _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
    private readonly IImagesStorageService _imagesBlobStorageService = imagesBlobStorageService ?? throw new ArgumentNullException(nameof(imagesBlobStorageService));
    private readonly IOCRService _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
    private readonly IImageProcessingTaskRepository _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));

    [Function(nameof(FuncImageProcessor))]
    public async Task RunAsync([ServiceBusTrigger("image-processing-queue", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
    {
        try
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            string messageBody = Encoding.UTF8.GetString(message.Body);
            var imageProcessingMessage = JsonConvert.DeserializeObject<ImageProcessingMessageDto>(messageBody);

            _logger.LogInformation($"Processing image: {imageProcessingMessage.ImageName} :: {imageProcessingMessage.Language} :: {imageProcessingMessage.StorageLocation}");

            // Create a new ImageProcessingTask instance
            var task = new ImageProcessingTask
            {
                RequestId = imageProcessingMessage.Id,
                ImageName = imageProcessingMessage.ImageName,
                Language = imageProcessingMessage.Language,
                StartTime = DateTime.UtcNow,
                Status = "Pending"
            };

            // Store the initial task in Cosmos DB
            await _taskRepository.CreateAsync(task);

            // Use the OCR service to perform OCR on the image
            var ocrResult = await _ocrService.ExtractTextFromImageAsync($"{imageProcessingMessage.StorageLocation}{imageProcessingMessage.ImageName}");

            // Process the OCR result as needed
            _logger.LogInformation($"Extracted Text: {ocrResult}");

            task.OCRResult = ocrResult;
            // Replace with actual extracted text
            task.ExtractedText = "This is a sample extracted text.";
            task.Status = "Processing";

            // Update the task in Cosmos DB
            await _taskRepository.UpdateAsync(task);

            // Call method to move image to processed container // Use Wait() since Azure Functions does not support async main
            await _imagesBlobStorageService.MoveImageToProcessedContainerAsync(imageProcessingMessage.ImageName);

            task.EndTime = DateTime.UtcNow;
            task.Status = "Completed";

            // Update the task in Cosmos DB
            await _taskRepository.UpdateAsync(task);
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogError(cosmosEx, $"BadRequest (400) error while processing message ID '{message.MessageId}' from queue: {cosmosEx.Message}");
            _logger.LogError($"Substatus: {cosmosEx.SubStatusCode}, ActivityId: {cosmosEx.ActivityId}");
            _logger.LogError($"Cosmos DB error details: {cosmosEx.ToString()}");

            throw; // Rethrow the exception to mark the function invocation as failed
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing message: {ex.Message}");
            throw;
        }
    }

}
