using Azure.Storage.Blobs;
using FuncApp_TextExtractor.Configuration;
using Microsoft.Extensions.Options;

namespace FuncApp_TextExtractor.ImagesBlobStorage;

public class ImagesBlobStorageService() : IImagesBlobStorageService
{
    //private readonly FunctionSettings _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

    public async Task MoveImageToProcessedContainerAsync(string imageName)
    {
        // Logic to move the image to the processed container
        string sourceContainerName = "incoming-images";
        string destinationContainerName = "processed-images";

        //// Create a BlobServiceClient object which will be used to create a container client
        //var blobServiceClient = new BlobServiceClient(_settings.BlobConnectionString);

        //// Get a reference to the source container
        //var sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainerName);

        //// Get a reference to the destination container
        //var destinationContainerClient = blobServiceClient.GetBlobContainerClient(destinationContainerName);

        //// Get a reference to the source blob
        //var sourceBlobClient = sourceContainerClient.GetBlobClient(imageName);

        //// Get a reference to the destination blob
        //var destinationBlobClient = destinationContainerClient.GetBlobClient(imageName);

        //// Copy the blob from the source to the destination
        //await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);

        //// Delete the blob from the source container
        //await sourceBlobClient.DeleteIfExistsAsync();
    }
}