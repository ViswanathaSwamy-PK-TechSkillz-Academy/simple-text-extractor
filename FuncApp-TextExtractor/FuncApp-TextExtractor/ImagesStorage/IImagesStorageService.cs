namespace FuncApp_TextExtractor.ImagesStorage;

public interface IImagesStorageService
{
    Task MoveImageToProcessedContainerAsync(string imageName);
}