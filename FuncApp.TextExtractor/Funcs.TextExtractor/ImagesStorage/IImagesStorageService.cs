namespace Funcs.TextExtractor.ImagesStorage;

public interface IImagesStorageService
{
    Task MoveImageToProcessedContainerAsync(string imageName);
}