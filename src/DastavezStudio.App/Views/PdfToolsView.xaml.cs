using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class PdfToolsView : Page
{
    public PdfToolsViewModel ViewModel { get; }

    public PdfToolsView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PdfToolsViewModel>();
    }

    private async void OnSelectFileClicked(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, App.WindowHandle);
        picker.ViewMode = PickerViewMode.List;
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".pdf");

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            await ViewModel.SetInputFileAsync(file.Path);
        }
    }

    private async void OnExportClicked(object sender, RoutedEventArgs e)
    {
        var sourceFile = ViewModel.ProcessedFilePath ?? ViewModel.InputFilePath;
        if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
        {
            ViewModel.StatusMessage = "No compressed PDF to save. Click 'Start Compression' first.";
            return;
        }

        var savePicker = new FileSavePicker();
        InitializeWithWindow.Initialize(savePicker, App.WindowHandle);
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("PDF Document", new List<string> { ".pdf" });
        savePicker.SuggestedFileName = $"Compressed_{Path.GetFileNameWithoutExtension(ViewModel.InputFileName ?? "document")}";

        var targetFile = await savePicker.PickSaveFileAsync();
        if (targetFile != null)
        {
            File.Copy(sourceFile, targetFile.Path, overwrite: true);
            ViewModel.StatusMessage = $"✓ Compressed PDF successfully saved to: {targetFile.Path}";
        }
    }
}
