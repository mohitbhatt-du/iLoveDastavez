using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using DastavezStudio.App.Helpers;
using DastavezStudio.App.ViewModels;
using DastavezStudio.App.Views;

namespace DastavezStudio.App;

public sealed partial class MainPage : Page
{
    public static MainPage? Current { get; private set; }

    public MainPageViewModel ViewModel { get; }

    public MainPage()
    {
        Current = this;
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<MainPageViewModel>();
    }

    private async void OnNavViewLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var logoSmallPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppLogo_Small.png");
            if (File.Exists(logoSmallPath))
            {
                var bmp = await ImagePreviewHelper.LoadBitmapFromFileAsync(logoSmallPath);
                if (bmp != null)
                {
                    NavHeaderLogo.Source = bmp;
                }
                else
                {
                    NavHeaderLogo.Source = new BitmapImage(new Uri(logoSmallPath));
                }
            }
        }
        catch
        {
        }

        NavView.SelectedItem = NavView.MenuItems[0];
        NavigateToTag("Home", null);
    }

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem selectedItem && selectedItem.Tag is string tag)
        {
            NavigateToTag(tag, null);
        }
    }

    public void NavigateTo(string tag, object? parameter = null)
    {
        foreach (var item in NavView.MenuItems)
        {
            if (item is NavigationViewItem nvi && (string)nvi.Tag == tag)
            {
                NavView.SelectedItem = nvi;
                NavigateToTag(tag, parameter);
                return;
            }
        }

        foreach (var item in NavView.FooterMenuItems)
        {
            if (item is NavigationViewItem nvi && (string)nvi.Tag == tag)
            {
                NavView.SelectedItem = nvi;
                NavigateToTag(tag, parameter);
                return;
            }
        }

        NavigateToTag(tag, parameter);
    }

    private void NavigateToTag(string tag, object? parameter)
    {
        Type? pageType = tag switch
        {
            "Home" => typeof(HomeView),
            "ExamStudio" => typeof(ExamStudioView),
            "PdfTools" => typeof(PdfToolsView),
            "ImageTools" => typeof(ImageToolsView),
            "Batch" => typeof(BatchView),
            "ScannerOcr" => typeof(ScannerOcrView),
            "AiEnhance" => typeof(AiEnhanceView),
            "Settings" => typeof(SettingsView),
            "About" => typeof(AboutView),
            _ => typeof(HomeView)
        };

        ContentFrame.Navigate(pageType, parameter);
    }
}
