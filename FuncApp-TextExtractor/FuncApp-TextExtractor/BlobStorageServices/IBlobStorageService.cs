namespace FuncApp_TextExtractor.BlobStorageServices;

public interface IBlobStorageService
{
    Task MoveImageToProcessedContainerAsync(string imageName);
}