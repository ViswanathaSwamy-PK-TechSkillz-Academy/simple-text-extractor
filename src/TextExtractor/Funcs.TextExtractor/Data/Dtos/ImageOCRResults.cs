using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funcs.TextExtractor.Data.Dtos;

public record ImageOCRResults
{
    public string OCRResult { get; set; } = string.Empty;

    public string ExtractedText { get; set; } = string.Empty;
}
