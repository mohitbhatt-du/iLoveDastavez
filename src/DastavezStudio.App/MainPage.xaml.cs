using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using DastavezStudio.App.ViewModels;
using DastavezStudio.App.Views;

namespace DastavezStudio.App;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; }

    public MainPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<MainPageViewModel>();
    }

    private void OnNavViewLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        NavView.SelectedItem = NavView.MenuItems[0];
        NavigateToTag("Home");
    }

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem selectedItem && selectedItem.Tag is string tag)
        {
            NavigateToTag(tag);
        }
    }

    private void NavigateToTag(string tag)
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

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
