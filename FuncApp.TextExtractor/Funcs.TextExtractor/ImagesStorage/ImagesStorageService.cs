using Azure.Storage.Blobs;
using Funcs.TextExtractor.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Funcs.TextExtractor.ImagesStorage;

public class ImagesStorageService(IOptions<FunctionSettings> settings, ILogger<ImagesStorageService> logger) : IImagesStorageService
{
    private readonly FunctionSettings _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
    private readonly ILogger<ImagesStorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task MoveImageToProcessedContainerAsync(string imageName)
    {
        // Logic to move the image to the processed container
        string sourceContainerName = "incoming-images";
        string destinationContainerName = "processed-images";

        // Create a BlobServiceClient object which will be used to create a container client
        var blobServiceClient = new BlobServiceClient(_settings.BlobConnectionString);

        // Get a reference to the source container
        var sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainerName);

        // Get a reference to the destination container
        var destinationContainerClient = blobServiceClient.GetBlobContainerClient(destinationContainerName);

        // Get a reference to the source blob
        var sourceBlobClient = sourceContainerClient.GetBlobClient(imageName);

        // Check if the source blob exists
        if (await sourceBlobClient.ExistsAsync())
        {
            // Get a reference to the destination blob
            var destinationBlobClient = destinationContainerClient.GetBlobClient(imageName);

            // Copy the blob from the source to the destination
            await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);

            // Delete the blob from the source container
            await sourceBlobClient.DeleteIfExistsAsync();
        }
        else
        {
            // Handle the case where the source blob does not exist
            // For example, log a message or throw an exception
            //throw new FileNotFoundException("Source blob does not exist.");
            _logger.LogError($"Source blob {imageName} does not exist.");
        }
    }
}