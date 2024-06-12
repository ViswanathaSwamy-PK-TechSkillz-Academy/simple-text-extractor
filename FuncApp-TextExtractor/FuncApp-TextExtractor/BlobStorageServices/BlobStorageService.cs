using Azure.Storage.Blobs;

namespace FuncApp_TextExtractor.BlobStorageServices;

public class BlobStorageService(string blobConnectionString) : IBlobStorageService
{
    private readonly string _blobConnectionString = blobConnectionString ?? throw new ArgumentNullException(nameof(blobConnectionString));

    public async Task MoveImageToProcessedContainerAsync(string imageName)
    {
        // Logic to move the image to the processed container
        string sourceContainerName = "incoming-images";
        string destinationContainerName = "processed-images";

        // Create a BlobServiceClient object which will be used to create a container client
        var blobServiceClient = new BlobServiceClient(_blobConnectionString);

        // Get a reference to the source container
        var sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainerName);

        // Get a reference to the destination container
        var destinationContainerClient = blobServiceClient.GetBlobContainerClient(destinationContainerName);

        // Get a reference to the source blob
        var sourceBlobClient = sourceContainerClient.GetBlobClient(imageName);

        // Get a reference to the destination blob
        var destinationBlobClient = destinationContainerClient.GetBlobClient(imageName);

        // Copy the blob from the source to the destination
        await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);

        // Delete the blob from the source container
        await sourceBlobClient.DeleteIfExistsAsync();
    }
}