using FileNexus.Database.Connection;
using FileNexus.Database.Repositories;
using FileNexus.Interop.Services;
using FileNexus.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileNexus.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileNexusServices(this IServiceCollection services, string? customDbPath = null)
    {
        // Database Layer
        services.AddSingleton<IDatabaseInitializer>(_ => new DatabaseInitializer(customDbPath));
        services.AddSingleton<IWorkspaceRepository, WorkspaceRepository>();
        services.AddSingleton<IFileRecordRepository, FileRecordRepository>();

        // Interop Layer
        services.AddSingleton<INativeScannerBridge, NativeScannerBridge>();

        // Core Business Layer
        services.AddSingleton<ICategoryClassifier, CategoryClassifier>();
        services.AddSingleton<IScannerService, ScannerService>();
        services.AddSingleton<IWorkspaceService, WorkspaceService>();
        services.AddSingleton<IFileService, FileService>();

        return services;
    }
}
