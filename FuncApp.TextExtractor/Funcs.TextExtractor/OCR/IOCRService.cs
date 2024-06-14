namespace Funcs.TextExtractor.OCR;

public interface IOCRService
{
    Task<string> ExtractTextFromImageAsync(string imageUrl);
}