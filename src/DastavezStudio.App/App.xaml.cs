using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using DastavezStudio.App.ViewModels;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Services;
using DastavezStudio.ExamPresets;
using DastavezStudio.ImageEngine;
using DastavezStudio.PdfEngine;
using DastavezStudio.Security;
using DastavezStudio.AiEngine;
using DastavezStudio.OcrEngine;

namespace DastavezStudio.App;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static Window Window { get; private set; } = null!;
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;
    public static nint WindowHandle => WinRT.Interop.WindowNative.GetWindowHandle(Window);
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Core Infrastructure & Engines
        services.AddSingleton<ITempFileManager, TempFileManager>();
        services.AddSingleton<IExamPresetService, ExamPresetService>();
        services.AddSingleton<IImageProcessor, ImageProcessor>();
        services.AddSingleton<IPdfProcessor, PdfProcessor>();
        services.AddSingleton<IBatchProcessor, BatchProcessor>();
        services.AddSingleton<IAiEnhancer, AiEnhancer>();
        services.AddSingleton<IDocumentCleaner, DocumentCleaner>();
        services.AddSingleton<IOcrEngine, WindowsOcrEngine>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IUpdateService, UpdateService>();

        // ViewModels
        services.AddTransient<MainPageViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<ExamStudioViewModel>();
        services.AddTransient<PdfToolsViewModel>();
        services.AddTransient<ImageToolsViewModel>();
        services.AddTransient<BatchViewModel>();
        services.AddTransient<ScannerOcrViewModel>();
        services.AddTransient<AiEnhanceViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<AboutViewModel>();

        Services = services.BuildServiceProvider();
    }

    public static MainWindow MainWindow => (MainWindow)Window;

    public static void SetTheme(ElementTheme theme)
    {
        if (Window?.Content is FrameworkElement root)
        {
            root.RequestedTheme = theme;
        }
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        var settings = Services.GetRequiredService<ISettingsService>();
        var theme = settings.Preferences.ThemeIndex switch
        {
            1 => ElementTheme.Light,
            2 => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
        SetTheme(theme);

        Window.Activate();
    }
}
