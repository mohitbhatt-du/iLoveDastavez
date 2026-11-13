using DastavezStudio.Core.Interfaces;

namespace DastavezStudio.Security;

public class TempFileManager : ITempFileManager
{
    private readonly string _sessionDirectory;
    private bool _disposed;

    public string SessionDirectory => _sessionDirectory;

    public TempFileManager()
    {
        var baseTemp = Path.Combine(Path.GetTempPath(), "DastavezStudio");
        _sessionDirectory = Path.Combine(baseTemp, $"Session_{Guid.NewGuid():N}");
        if (!Directory.Exists(_sessionDirectory))
        {
            Directory.CreateDirectory(_sessionDirectory);
        }
    }

    public string GetTempFilePath(string extension)
    {
        if (!extension.StartsWith("."))
            extension = "." + extension;
        return Path.Combine(_sessionDirectory, $"file_{Guid.NewGuid():N}{extension}");
    }

    public void CleanupSession()
    {
        try
        {
            if (Directory.Exists(_sessionDirectory))
            {
                Directory.Delete(_sessionDirectory, recursive: true);
            }
        }
        catch
        {
            // Suppress errors during best-effort cleanup on shutdown
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            CleanupSession();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    ~TempFileManager()
    {
        Dispose();
    }
}
