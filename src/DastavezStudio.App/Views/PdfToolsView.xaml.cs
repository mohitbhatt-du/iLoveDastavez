using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class PdfToolsView : Page
{
    public PdfToolsViewModel ViewModel { get; }

    public PdfToolsView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PdfToolsViewModel>();
    }
}
