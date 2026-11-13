using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DastavezStudio.App.ViewModels;

public class BatchFileItem : ObservableObject
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public double Progress { get; set; }
}

public partial class BatchViewModel : ObservableObject
{
    public ObservableCollection<BatchFileItem> Queue { get; } = new();

    [ObservableProperty]
    public partial double OverallProgress { get; set; }

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Add images or documents to begin batch processing.";
}
