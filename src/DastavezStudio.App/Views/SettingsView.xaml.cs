using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
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

    private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is RadioButtons rb && rb.SelectedIndex >= 0)
        {
            ViewModel.ApplyTheme(rb.SelectedIndex);
        }
    }

    private void OnSystemThemeClicked(object sender, RoutedEventArgs e)
    {
        ThemeSelector.SelectedIndex = 0;
        ViewModel.ApplyTheme(0);
    }

    private void OnLightThemeClicked(object sender, RoutedEventArgs e)
    {
        ThemeSelector.SelectedIndex = 1;
        ViewModel.ApplyTheme(1);
    }

    private void OnDarkThemeClicked(object sender, RoutedEventArgs e)
    {
        ThemeSelector.SelectedIndex = 2;
        ViewModel.ApplyTheme(2);
    }
}
