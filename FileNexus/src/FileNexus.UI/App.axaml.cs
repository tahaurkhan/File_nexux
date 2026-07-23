using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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

    public override async void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddFileNexusServices();
        serviceCollection.AddTransient<MainWindowViewModel>();

        Services = serviceCollection.BuildServiceProvider();

        // Initialize SQLite Database Context WAL mode asynchronously
        var dbInitializer = Services.GetRequiredService<IDatabaseInitializer>();
        await dbInitializer.InitializeAsync();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}