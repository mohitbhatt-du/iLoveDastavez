using CommunityToolkit.Mvvm.ComponentModel;

namespace DastavezStudio.App.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    public string AppName => "Dastavez Studio";
    public string Publisher => "iLoveDastavez";
    public string Tagline => "हर दस्तावेज़, तैयार!";
    public string Version => "v1.0.0 (LTS Release)";
    public string Platform => ".NET 10.0 • WinUI 3 • Windows App SDK";

    public string PrivacyPolicySummary =>
        "Dastavez Studio runs 100% locally on your PC. No documents, photos, signatures, or personal identity files are ever uploaded or transmitted over the network. Zero analytics. Zero telemetry.";

    public string Disclaimer =>
        "Examination authority names and logos (such as IBPS, SBI, SSC, UPSC, RRB, NTA) are the property of their respective governing bodies. Dastavez Studio is an independent document toolkit and is not affiliated with, authorized, or endorsed by these organizations. Requirements are provided for convenience and must always be cross-verified against official notifications.";
}
