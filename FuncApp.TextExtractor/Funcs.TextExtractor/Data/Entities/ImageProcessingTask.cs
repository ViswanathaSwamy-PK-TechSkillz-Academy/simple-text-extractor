using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}