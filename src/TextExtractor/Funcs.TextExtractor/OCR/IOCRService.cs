using Funcs.TextExtractor.Data.Dtos;

namespace Funcs.TextExtractor.OCR;

public interface IOCRService
{
    Task<ImageOCRResults> ExtractTextFromImageAsync(string imageUrl);
}