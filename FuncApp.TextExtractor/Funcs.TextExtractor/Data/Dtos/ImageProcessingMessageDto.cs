namespace Funcs.TextExtractor.Data.Dtos;

public class ImageProcessingMessageDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ImageName { get; set; } = string.Empty;

    public string Language { get; set; } = "en";

    public string StorageLocation { get; set; } = string.Empty;
}
