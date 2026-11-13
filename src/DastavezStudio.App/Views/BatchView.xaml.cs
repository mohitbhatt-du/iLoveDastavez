using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class BatchView : Page
{
    public BatchViewModel ViewModel { get; }

    public BatchView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<BatchViewModel>();
    }
}
