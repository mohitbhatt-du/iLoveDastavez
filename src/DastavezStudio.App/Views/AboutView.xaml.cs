using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class AboutView : Page
{
    public AboutViewModel ViewModel { get; }

    public AboutView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<AboutViewModel>();
    }
}
