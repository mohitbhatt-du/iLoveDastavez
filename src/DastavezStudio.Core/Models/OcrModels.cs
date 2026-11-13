namespace DastavezStudio.Core.Models;

public class OcrWord
{
    public string Text { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Confidence { get; set; } = 1.0;
}

public class OcrLine
{
    public string Text { get; set; } = string.Empty;
    public IReadOnlyList<OcrWord> Words { get; set; } = Array.Empty<OcrWord>();
}

public class OcrResult
{
    public string FullText { get; set; } = string.Empty;
    public IReadOnlyList<OcrLine> Lines { get; set; } = Array.Empty<OcrLine>();
    public double? TextAngleDegrees { get; set; }
    public TimeSpan Duration { get; set; }
    public string Language { get; set; } = "en-US";
}

public class DocumentCleanOptions
{
    public bool AutoDeskew { get; set; } = true;
    public bool RemoveShadows { get; set; } = true;
    public bool BinarizePureBlackAndWhite { get; set; } = true;
    public int Threshold { get; set; } = 128;
    public bool EnhanceContrast { get; set; } = true;
    public bool Denoise { get; set; } = true;
}

public class DocumentCleanResult
{
    public string OutputPath { get; set; } = string.Empty;
    public double DetectedSkewAngleDegrees { get; set; }
    public bool ShadowsRemoved { get; set; }
    public bool Binarized { get; set; }
    public TimeSpan Duration { get; set; }
}
