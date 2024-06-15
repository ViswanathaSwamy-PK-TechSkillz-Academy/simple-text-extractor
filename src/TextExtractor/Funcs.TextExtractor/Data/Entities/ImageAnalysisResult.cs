namespace Funcs.TextExtractor.Data.Entities;

using System.Collections.Generic;

public class Metadata
{
    public int Width { get; set; }

    public int Height { get; set; }
}

public class Word
{
    public string Text { get; set; }

    public List<Point> BoundingPolygon { get; set; }

    public double Confidence { get; set; }
}

public class Line
{
    public string Text { get; set; }

    public List<Point> BoundingPolygon { get; set; }

    public List<Word> Words { get; set; }
}

public class Block
{
    public List<Line> Lines { get; set; }
}

public class ReadResult
{
    public List<Block> Blocks { get; set; }
}

public class ImageAnalysisResult
{
    public string ModelVersion { get; set; }

    public Metadata Metadata { get; set; }

    public ReadResult ReadResult { get; set; }
}

public class Point
{
    public int X { get; set; }

    public int Y { get; set; }
}
