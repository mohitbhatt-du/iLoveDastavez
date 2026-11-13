using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace DastavezStudio.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        var icoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        if (File.Exists(icoPath))
        {
            AppWindow.SetIcon(icoPath);
        }

        var smallLogoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppLogo_Small.png");
        if (File.Exists(smallLogoPath))
        {
            AppTitleBar.IconSource = new ImageIconSource { ImageSource = new BitmapImage(new Uri(smallLogoPath)) };
        }

        // Navigate the root frame to the main page on startup.
        RootFrame.Navigate(typeof(MainPage));
    }
}
