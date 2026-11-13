using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
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
}
