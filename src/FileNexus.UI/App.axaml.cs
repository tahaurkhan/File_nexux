using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FileNexus.Core.DependencyInjection;
using FileNexus.Database.Connection;
using FileNexus.UI.ViewModels;
using FileNexus.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FileNexus.UI;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Show splash screen immediately on the UI thread
            var splash = new SplashWindow();
            desktop.MainWindow = splash;
            splash.Show();

            // Run heavy initialization in background, then switch to main window
            Task.Run(async () =>
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                    splash.SetStatus("Loading services…", 0.1));

                await Task.Delay(300);

                // Build DI container
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddFileNexusServices();
                serviceCollection.AddTransient<MainWindowViewModel>();
                Services = serviceCollection.BuildServiceProvider();

                await Dispatcher.UIThread.InvokeAsync(() =>
                    splash.SetStatus("Applying theme…", 0.35));

                await Task.Delay(200);

                // Initialize theme
                FileNexus.UI.Services.ThemeManager.Initialize();

                await Dispatcher.UIThread.InvokeAsync(() =>
                    splash.SetStatus("Initializing database…", 0.55));

                await Task.Delay(100);

                // Initialize SQLite database
                var dbInitializer = Services.GetRequiredService<IDatabaseInitializer>();
                try
                {
                    await dbInitializer.InitializeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database initialization error: {ex.Message}");
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                    splash.SetStatus("Loading workspace…", 0.80));

                await Task.Delay(250);

                await Dispatcher.UIThread.InvokeAsync(() =>
                    splash.SetStatus("Ready!", 1.0));

                await Task.Delay(400);

                // Switch to main window on the UI thread
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var viewModel = Services.GetRequiredService<MainWindowViewModel>();
                    var mainWindow = new MainWindow
                    {
                        DataContext = viewModel
                    };

                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                    splash.Close();
                });
            });
        }

        base.OnFrameworkInitializationCompleted();
    }
}