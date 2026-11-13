using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class ImageToolsView : Page
{
    public ImageToolsViewModel ViewModel { get; }

    public ImageToolsView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ImageToolsViewModel>();
    }
}
