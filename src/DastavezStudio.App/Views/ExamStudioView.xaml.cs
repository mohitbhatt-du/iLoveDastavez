using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class ExamStudioView : Page
{
    public ExamStudioViewModel ViewModel { get; }

    public ExamStudioView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ExamStudioViewModel>();
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
            return;
        }

        var savePicker = new FileSavePicker();
        InitializeWithWindow.Initialize(savePicker, App.WindowHandle);
        savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        savePicker.FileTypeChoices.Add("JPEG Image", new List<string> { ".jpg" });
        savePicker.SuggestedFileName = $"Conformed_{ViewModel.SelectedPreset?.Authority}_{Path.GetFileNameWithoutExtension(sourceFile)}";

        var targetFile = await savePicker.PickSaveFileAsync();
        if (targetFile != null)
        {
            File.Copy(sourceFile, targetFile.Path, overwrite: true);
            ViewModel.StatusMessage = $"✓ Document successfully saved to: {targetFile.Path}";
        }
    }
}
