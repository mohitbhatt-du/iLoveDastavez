using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class AiEnhanceView : Page
{
    public AiEnhanceViewModel ViewModel { get; }

    public AiEnhanceView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<AiEnhanceViewModel>();
    }

    private async void OnSelectFileClicked(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, App.WindowHandle);
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            ViewModel.SetInputFile(file.Path);
        }
    }

    private async void OnExportClicked(object sender, RoutedEventArgs e)
    {
        var source = ViewModel.EnhancedFilePath ?? ViewModel.InputFilePath;
        if (string.IsNullOrEmpty(source) || !File.Exists(source)) return;

        var savePicker = new FileSavePicker();
        InitializeWithWindow.Initialize(savePicker, App.WindowHandle);
        savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        savePicker.FileTypeChoices.Add("JPEG Image", new List<string> { ".jpg" });
        savePicker.FileTypeChoices.Add("PNG Image", new List<string> { ".png" });
        savePicker.SuggestedFileName = $"Enhanced_{Path.GetFileNameWithoutExtension(source)}";

        var targetFile = await savePicker.PickSaveFileAsync();
        if (targetFile != null)
        {
            File.Copy(source, targetFile.Path, overwrite: true);
            ViewModel.StatusMessage = $"✓ Enhanced image exported to: {targetFile.Path}";
        }
    }
}
