namespace FuncApp_TextExtractor.OCR;

public interface IOCRService
{
    Task<string> ExtractTextFromImageAsync(string imageUrl);
}