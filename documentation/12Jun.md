# 12-Jun

Great! Now that we have all the relevant pieces of code, let's summarize and ensure that each part of the solution is correctly configured and implemented. The code you've provided sets up a robust Azure Function for processing images with OCR using Azure Cognitive Services.

## Project Structure and Code Overview

1. **Configuration**:

   - `FunctionSettings`: Holds configuration settings like Blob storage connection strings, Cosmos DB credentials, and Cognitive Services credentials.

2. **Data Transfer Objects**:

   - `ImageProcessingMessageDto`: Represents the message structure used in the Service Bus queue.

3. **Image Storage Service**:

   - `IImagesStorageService`: Interface for image storage operations.
   - `ImagesStorageService`: Implementation that handles moving images between Blob storage containers.

4. **OCR Service**:

   - `IOCRService`: Interface for the OCR service.
   - `AzureComputerVisionOCRService`: Implementation using Azure Cognitive Services to extract text from images.

5. **Function Implementation**:

   - `FuncImageProcessor`: Azure Function triggered by Service Bus messages, performs OCR, and handles image storage.

6. **Function App Configuration**:
   - Program entry point for configuring services and running the Azure Function.

## Complete Code Implementation

**1. Configuration:**

`FunctionSettings.cs`:

```csharp
namespace Funcs.TextExtractor.Configuration
{
    public class FunctionSettings
    {
        public string BlobConnectionString { get; set; } = string.Empty;
        public string CosmosDbEndpoint { get; set; } = string.Empty;
        public string CosmosDbKey { get; set; } = string.Empty;
        public string CosmosDbDatabaseId { get; set; } = string.Empty;
        public string CosmosDbContainerId { get; set; } = string.Empty;
        public string AzAiServicesEndpoint { get; set; } = string.Empty;
        public string AzAiServicesApiKey { get; set; } = string.Empty;
    }
}
```

**2. Data Transfer Objects:**

`ImageProcessingMessageDto.cs`:

```csharp
namespace Funcs.TextExtractor.Data.Dtos
{
    public class ImageProcessingMessageDto
    {
        public string ImageName { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string StorageLocation { get; set; } = string.Empty;
    }
}
```

**3. Image Storage Service:**

`IImagesStorageService.cs`:

```csharp
namespace Funcs.TextExtractor.ImagesStorage
{
    public interface IImagesStorageService
    {
        Task MoveImageToProcessedContainerAsync(string imageName);
    }
}
```

`ImagesStorageService.cs`:

```csharp
using Azure.Storage.Blobs;
using Funcs.TextExtractor.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Funcs.TextExtractor.ImagesStorage
{
    public class ImagesStorageService : IImagesStorageService
    {
        private readonly FunctionSettings _settings;
        private readonly ILogger<ImagesStorageService> _logger;

        public ImagesStorageService(IOptions<FunctionSettings> settings, ILogger<ImagesStorageService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task MoveImageToProcessedContainerAsync(string imageName)
        {
            string sourceContainerName = "incoming-images";
            string destinationContainerName = "processed-images";

            var blobServiceClient = new BlobServiceClient(_settings.BlobConnectionString);
            var sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainerName);
            var destinationContainerClient = blobServiceClient.GetBlobContainerClient(destinationContainerName);
            var sourceBlobClient = sourceContainerClient.GetBlobClient(imageName);

            if (await sourceBlobClient.ExistsAsync())
            {
                var destinationBlobClient = destinationContainerClient.GetBlobClient(imageName);
                await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);
                await sourceBlobClient.DeleteIfExistsAsync();
            }
            else
            {
                _logger.LogError($"Source blob {imageName} does not exist.");
            }
        }
    }
}
```

**4. OCR Service:**

`IOCRService.cs`:

```csharp
namespace Funcs.TextExtractor.OCR
{
    public interface IOCRService
    {
        Task<string> ExtractTextFromImageAsync(string imageUrl);
    }
}
```

`AzureComputerVisionOCRService.cs`:

```csharp
using Funcs.TextExtractor.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace Funcs.TextExtractor.OCR
{
    public class AzureComputerVisionOCRService : IOCRService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _endpoint;
        private readonly string _apiKey;

        public AzureComputerVisionOCRService(IOptions<FunctionSettings> options, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _endpoint = options.Value.AzAiServicesEndpoint;
            _apiKey = options.Value.AzAiServicesApiKey;
        }

        public async Task<string> ExtractTextFromImageAsync(string imageUrl)
        {
            var client = _clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/computervision/imageanalysis:analyze?api-version=2024-02-01&features=read&language=en");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);
            var content = new StringContent($"{{\"url\": \"{imageUrl}\"}}", null, "application/json");
            request.Content = content;

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Failed to extract text from image. Status code: {response.StatusCode}");
            }
        }
    }
}
```

**5. Function Implementation:**

`FuncImageProcessor.cs`:

```csharp
using Azure.Messaging.ServiceBus;
using Funcs.TextExtractor.Configuration;
using Funcs.TextExtractor.Data.Dtos;
using Funcs.TextExtractor.ImagesStorage;
using Funcs.TextExtractor.OCR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Funcs.TextExtractor
{
    public class FuncImageProcessor
    {
        private readonly ILogger<FuncImageProcessor> _logger;
        private readonly FunctionSettings _settings;
        private readonly IImagesStorageService _imagesBlobStorageService;
        private readonly IOCRService _ocrService;

        public FuncImageProcessor(ILogger<FuncImageProcessor> logger, IOptions<FunctionSettings> settings, IImagesStorageService imagesBlobStorageService, IOCRService ocrService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _imagesBlobStorageService = imagesBlobStorageService ?? throw new ArgumentNullException(nameof(imagesBlobStorageService));
            _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        }

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

                var ocrResult = _ocrService.ExtractTextFromImageAsync($"{imageProcessingMessage.StorageLocation}{imageProcessingMessage.ImageName}").Result;

                _logger.LogInformation($"Extracted Text: {ocrResult}");

                _imagesBlobStorageService.MoveImageToProcessedContainerAsync(imageProcessingMessage.ImageName).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
                throw;
            }
        }
    }
}
```

**6. Function App Configuration:**

`Program.cs`:

```csharp
using Funcs.TextExtractor.Configuration;
using Funcs.TextExtractor.ImagesStorage;
using Funcs.TextExtractor.OCR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var config = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        services.Configure<FunctionSettings>(config.GetSection("FunctionSettings"));

        services.AddHttpClient();
        services.AddTransient<IImagesStorageService, ImagesStorageService>();
        services.AddTransient<IOCRService, AzureComputerVisionOCRService>();
    })
    .Build();

host.Run();
```

### Configuration File

Ensure you have a `local.settings.json` file with the necessary configurations:

`local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "Your_AzureWebJobsStorage_ConnectionString",

    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ServiceBusConnection": "Your_ServiceBus_ConnectionString",
    "ApplicationInsights:InstrumentationKey": "Your_ApplicationInsights_InstrumentationKey"
  },
  "FunctionSettings": {
    "BlobConnectionString": "Your_Blob_ConnectionString",
    "CosmosDbEndpoint": "Your_CosmosDb_Endpoint",
    "CosmosDbKey": "Your_CosmosDb_Key",
    "CosmosDbDatabaseId": "Your_CosmosDb_DatabaseId",
    "CosmosDbContainerId": "Your_CosmosDb_ContainerId",
    "AzAiServicesEndpoint": "Your_AzAiServices_Endpoint",
    "AzAiServicesApiKey": "Your_AzAiServices_ApiKey"
  }
}
```

### Final Steps

1. **Deploy the Function**: Deploy your function to Azure using Visual Studio or the Azure CLI.
2. **Test the Function**: Upload images to the `incoming-images` Blob container, and enqueue messages to the `image-processing-queue` to see the function in action.

This completes the implementation of the Azure OCR Image Processing Solution. If you have any further questions or need additional help, feel free to ask!
