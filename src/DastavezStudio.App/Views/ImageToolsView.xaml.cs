using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using WinRT.Interop;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class ImageToolsView : Page
{
    public ImageToolsViewModel ViewModel { get; }

    public ImageToolsView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ImageToolsViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string filePath && File.Exists(filePath))
        {
            await ViewModel.SetInputFileAsync(filePath);
        }
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
        picker.FileTypeFilter.Add(".bmp");

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
            ViewModel.StatusMessage = "No processed image to save. Click 'Process Image' first.";
            return;
        }

        var savePicker = new FileSavePicker();
        InitializeWithWindow.Initialize(savePicker, App.WindowHandle);
        savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        var ext = Path.GetExtension(sourceFile).ToLowerInvariant();
        savePicker.FileTypeChoices.Add("Image File", new List<string> { ext });
        savePicker.SuggestedFileName = $"Processed_{Path.GetFileNameWithoutExtension(ViewModel.InputFileName ?? "image")}";

        var targetFile = await savePicker.PickSaveFileAsync();
        if (targetFile != null)
        {
            File.Copy(sourceFile, targetFile.Path, overwrite: true);
            ViewModel.StatusMessage = $"✓ Image successfully saved to: {targetFile.Path}";
        }
    }
}
