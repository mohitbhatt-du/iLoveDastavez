namespace DastavezStudio.Core.Interfaces;

public interface ITempFileManager : IDisposable
{
    string SessionDirectory { get; }
    string GetTempFilePath(string extension);
    void CleanupSession();
}
