namespace FuncApp.TextExtractor.Data.Dtos;

public class ImageProcessingMessageDto
{
    public string ImageName { get; set; } = string.Empty;

    public string Language { get; set; } = "en";

    public string StorageLocation { get; set; } = string.Empty;
}
