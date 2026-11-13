using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class HomeView : Page
{
    public HomeViewModel ViewModel { get; }

    public HomeView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<HomeViewModel>();
    }
}
