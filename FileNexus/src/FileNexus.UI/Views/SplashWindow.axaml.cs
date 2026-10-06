using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace FileNexus.UI.Views;

public partial class SplashWindow : Window
{
    private ProgressBar? _progressBar;
    private TextBlock? _statusText;

    public SplashWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _progressBar = this.FindControl<ProgressBar>("SplashProgressBar");
        _statusText = this.FindControl<TextBlock>("StatusText");
    }

    public void UpdateProgress(string status, double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_statusText != null) _statusText.Text = status;
            if (_progressBar != null) _progressBar.Value = Math.Clamp(progress, 0.0, 1.0);
        });
    }
}
