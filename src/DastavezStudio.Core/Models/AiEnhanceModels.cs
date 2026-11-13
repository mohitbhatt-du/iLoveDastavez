namespace DastavezStudio.Core.Models;

public class AiEnhanceOptions
{
    public int UpscaleFactor { get; set; } = 2; // 2 or 4
    public bool Denoise { get; set; } = true;
    public bool Sharpen { get; set; } = true;
    public bool EnhanceContrast { get; set; } = true;
    public float DenoiseStrength { get; set; } = 0.5f;
    public float SharpenStrength { get; set; } = 0.6f;
    public bool UseGpuAcceleration { get; set; } = false;
}

public class AiEnhanceResult
{
    public string OutputPath { get; set; } = string.Empty;
    public int OriginalWidth { get; set; }
    public int OriginalHeight { get; set; }
    public int EnhancedWidth { get; set; }
    public int EnhancedHeight { get; set; }
    public TimeSpan Duration { get; set; }
    public string MethodUsed { get; set; } = string.Empty;
}
