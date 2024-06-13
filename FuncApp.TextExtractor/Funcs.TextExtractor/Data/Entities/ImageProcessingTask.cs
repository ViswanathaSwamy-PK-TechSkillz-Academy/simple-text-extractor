namespace Funcs.TextExtractor.Data.Entities;

public class ImageProcessingTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ImageName { get; set; } = string.Empty;

    public string Language { get; set; } = "en";

    // Pending, Processing, Completed, Failed
    public string Status { get; set; } = "Pending";

    public string OCRResult { get; set; } = string.Empty;

    public string ExtractedText { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    // Partition key for Cosmos DB // Make sure this matches the Cosmos DB partition key definition
    public string RequestId { get; set; } = string.Empty;
}