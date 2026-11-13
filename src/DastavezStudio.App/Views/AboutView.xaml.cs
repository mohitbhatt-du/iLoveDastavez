using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using DastavezStudio.App.Helpers;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class AboutView : Page
{
    public AboutViewModel ViewModel { get; }

    public AboutView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<AboutViewModel>();
        Loaded += OnAboutViewLoaded;
    }

    private async void OnAboutViewLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppLogo.png");
            if (File.Exists(logoPath))
            {
                var bmp = await ImagePreviewHelper.LoadBitmapFromFileAsync(logoPath);
                if (bmp != null)
                {
                    AboutLogo.Source = bmp;
                }
                else
                {
                    AboutLogo.Source = new BitmapImage(new Uri(logoPath));
                }
            }
        }
        catch
        {
        }
    }
}
