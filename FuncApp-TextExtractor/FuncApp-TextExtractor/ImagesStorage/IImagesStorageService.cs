namespace FuncApp_TextExtractor.ImagesBlobStorage;

public interface IImagesStorageService
{
    Task MoveImageToProcessedContainerAsync(string imageName);
}