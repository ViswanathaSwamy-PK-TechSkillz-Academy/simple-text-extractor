namespace FuncApp_TextExtractor.ImagesBlobStorage;

public interface IImagesBlobStorageService
{
    Task MoveImageToProcessedContainerAsync(string imageName);
}