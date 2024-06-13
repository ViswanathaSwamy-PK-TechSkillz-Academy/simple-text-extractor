namespace FuncApp.TextExtractor.ImagesStorage;

public interface IImagesStorageService
{
    Task MoveImageToProcessedContainerAsync(string imageName);
}