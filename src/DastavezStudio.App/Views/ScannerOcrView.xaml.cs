using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class ScannerOcrView : Page
{
    public ScannerOcrViewModel ViewModel { get; }

    public ScannerOcrView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ScannerOcrViewModel>();
    }
}
