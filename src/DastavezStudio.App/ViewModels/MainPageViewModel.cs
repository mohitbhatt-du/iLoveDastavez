using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DastavezStudio.App.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string BrandName { get; set; } = "iLoveDastavez";

    [ObservableProperty]
    public partial string AppName { get; set; } = "Dastavez Studio";

    [ObservableProperty]
    public partial string Tagline { get; set; } = "हर दस्तावेज़, तैयार!";

    [ObservableProperty]
    public partial string Version { get; set; } = "v1.0.0 (LTS)";

    [ObservableProperty]
    public partial string PrivacyBadge { get; set; } = "100% Offline • Zero Cloud • Local Processing Only";

    public List<string> SupportedAuthorities { get; } = new()
    {
        "IBPS", "SBI", "SSC", "UPSC", "RRB", "NTA", "State PSC"
    };
}
