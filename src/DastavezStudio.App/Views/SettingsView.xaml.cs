using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class SettingsView : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
    }
}
