using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using DastavezStudio.App.ViewModels;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.ExamPresets;
using DastavezStudio.Security;

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

        // Core Infrastructure
        services.AddSingleton<ITempFileManager, TempFileManager>();
        services.AddSingleton<IExamPresetService, ExamPresetService>();

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

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        Window.Activate();
    }
}
