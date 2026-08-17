using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace FileNexus.UI.Views;

public partial class SplashWindow : Window
{
    private Border? _loadingBar;
    private TextBlock? _statusText;

    public SplashWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _loadingBar = this.FindControl<Border>("LoadingBar");
        _statusText = this.FindControl<TextBlock>("StatusText");
    }

    public void SetStatus(string message, double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_statusText != null)
                _statusText.Text = message;

            if (_loadingBar != null)
                _loadingBar.Width = 200.0 * Math.Clamp(progress, 0.0, 1.0);
        });
    }
}
