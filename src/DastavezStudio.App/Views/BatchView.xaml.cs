using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;
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

    private async void OnAddFilesClicked(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, App.WindowHandle);
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".pdf");

        var files = await picker.PickMultipleFilesAsync();
        if (files != null && files.Count > 0)
        {
            ViewModel.AddFiles(files.Select(f => f.Path));
        }
    }

    private async void OnAddFolderClicked(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        InitializeWithWindow.Initialize(picker, App.WindowHandle);
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            var supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            var found = Directory.EnumerateFiles(folder.Path, "*.*", SearchOption.TopDirectoryOnly)
                                 .Where(f => supported.Contains(Path.GetExtension(f)));
            ViewModel.AddFiles(found);
        }
    }

    private async void OnBrowseOutputFolderClicked(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        InitializeWithWindow.Initialize(picker, App.WindowHandle);
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            ViewModel.OutputDirectory = folder.Path;
        }
    }
}
