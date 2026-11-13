using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class ScannerOcrView : Page
{
    public ScannerOcrViewModel ViewModel { get; }

    public ScannerOcrView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ScannerOcrViewModel>();
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

    private async void OnExportCleanedClicked(object sender, RoutedEventArgs e)
    {
        var source = ViewModel.CleanedFilePath ?? ViewModel.InputFilePath;
        if (string.IsNullOrEmpty(source) || !File.Exists(source)) return;

        var savePicker = new FileSavePicker();
        InitializeWithWindow.Initialize(savePicker, App.WindowHandle);
        savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        savePicker.FileTypeChoices.Add("PNG Clean Image", new List<string> { ".png" });
        savePicker.FileTypeChoices.Add("JPEG Image", new List<string> { ".jpg" });
        savePicker.SuggestedFileName = $"Cleaned_{Path.GetFileNameWithoutExtension(source)}";

        var targetFile = await savePicker.PickSaveFileAsync();
        if (targetFile != null)
        {
            File.Copy(source, targetFile.Path, overwrite: true);
            ViewModel.StatusMessage = $"✓ Cleaned document exported to: {targetFile.Path}";
        }
    }

    private async void OnExportPdfClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ViewModel.SearchablePdfPath) || !File.Exists(ViewModel.SearchablePdfPath))
        {
            await ViewModel.CreateSearchablePdfCommand.ExecuteAsync(null);
        }

        if (string.IsNullOrEmpty(ViewModel.SearchablePdfPath) || !File.Exists(ViewModel.SearchablePdfPath))
            return;

        var savePicker = new FileSavePicker();
        InitializeWithWindow.Initialize(savePicker, App.WindowHandle);
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Searchable PDF Document", new List<string> { ".pdf" });
        savePicker.SuggestedFileName = $"Searchable_{Path.GetFileNameWithoutExtension(ViewModel.InputFileName)}";

        var targetFile = await savePicker.PickSaveFileAsync();
        if (targetFile != null)
        {
            File.Copy(ViewModel.SearchablePdfPath, targetFile.Path, overwrite: true);
            ViewModel.StatusMessage = $"✓ Searchable PDF exported to: {targetFile.Path}";
        }
    }

    private async void OnExportTxtClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ViewModel.ExtractedText)) return;

        var savePicker = new FileSavePicker();
        InitializeWithWindow.Initialize(savePicker, App.WindowHandle);
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Plain Text", new List<string> { ".txt" });
        savePicker.SuggestedFileName = $"ExtractedText_{Path.GetFileNameWithoutExtension(ViewModel.InputFileName)}";

        var targetFile = await savePicker.PickSaveFileAsync();
        if (targetFile != null)
        {
            await File.WriteAllTextAsync(targetFile.Path, ViewModel.ExtractedText);
            ViewModel.StatusMessage = $"✓ Extracted text saved to: {targetFile.Path}";
        }
    }
}
