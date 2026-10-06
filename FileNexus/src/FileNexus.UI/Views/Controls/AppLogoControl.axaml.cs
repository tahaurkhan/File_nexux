using Avalonia;
using Avalonia.Controls;

namespace FileNexus.UI.Views.Controls;

public partial class AppLogoControl : UserControl
{
    public static readonly StyledProperty<CornerRadius> LogoCornerRadiusProperty =
        AvaloniaProperty.Register<AppLogoControl, CornerRadius>(nameof(LogoCornerRadius), new CornerRadius(8));

    public CornerRadius LogoCornerRadius
    {
        get => GetValue(LogoCornerRadiusProperty);
        set => SetValue(LogoCornerRadiusProperty, value);
    }

    public AppLogoControl()
    {
        InitializeComponent();
    }
}
