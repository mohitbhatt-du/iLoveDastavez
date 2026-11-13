using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using DastavezStudio.App.Helpers;
using DastavezStudio.App.ViewModels;

namespace DastavezStudio.App.Views;

public sealed partial class HomeView : Page
{
    public HomeViewModel ViewModel { get; }

    public HomeView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<HomeViewModel>();
        Loaded += OnHomeViewLoaded;
    }

    private async void OnHomeViewLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppLogo.png");
            if (File.Exists(logoPath))
            {
                var bmp = await ImagePreviewHelper.LoadBitmapFromFileAsync(logoPath);
                if (bmp != null)
                {
                    HeroLogo.Source = bmp;
                }
                else
                {
                    HeroLogo.Source = new BitmapImage(new Uri(logoPath));
                }
            }
        }
        catch
        {
            // Fallback handled by XAML
        }
    }

    private void OnQuickActionItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is QuickActionItem item && !string.IsNullOrEmpty(item.Tag))
        {
            MainPage.Current?.NavigateTo(item.Tag);
        }
    }

    private void OnDropZoneDragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Open in Dastavez Studio";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;

            DropZone.BorderBrush = (Brush)Application.Current.Resources["SystemFillColorSuccessBrush"];
            DropZoneIcon.Foreground = (Brush)Application.Current.Resources["SystemFillColorSuccessBrush"];
            DropZoneTitle.Text = "Drop file to open...";
        }
    }

    private void OnDropZoneDragLeave(object sender, DragEventArgs e)
    {
        ResetDropZoneVisuals();
    }

    private async void OnDropZoneDrop(object sender, DragEventArgs e)
    {
        ResetDropZoneVisuals();

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0 && items[0] is StorageFile file)
            {
                RouteFile(file.Path);
            }
        }
    }

    private async void OnDropZoneClicked(object sender, PointerRoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, App.WindowHandle);
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".pdf");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".tiff");

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            RouteFile(file.Path);
        }
    }

    private void RouteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (ext == ".pdf")
        {
            MainPage.Current?.NavigateTo("PdfTools", filePath);
        }
        else
        {
            MainPage.Current?.NavigateTo("ExamStudio", filePath);
        }
    }

    private void ResetDropZoneVisuals()
    {
        DropZone.BorderBrush = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
        DropZoneIcon.Foreground = (Brush)Application.Current.Resources["AccentTextFillColorPrimaryBrush"];
        DropZoneTitle.Text = "Drag and drop documents or images here (or click to browse)";
    }
}
