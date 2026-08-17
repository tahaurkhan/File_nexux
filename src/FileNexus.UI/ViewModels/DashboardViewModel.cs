using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileNexus.Core.Services;
using FileNexus.Shared.Enums;
using FileNexus.Shared.Models;

namespace FileNexus.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IWorkspaceService? _workspaceService;
    private readonly IFileService? _fileService;
    private readonly Action<string>? _onNavigateSection;

    [ObservableProperty]
    public partial string GreetingText { get; set; } = "Welcome Back";

    [ObservableProperty]
    public partial string GreetingSubtext { get; set; } = "Here is your system storage & virtual file breakdown";

    [ObservableProperty]
    public partial ObservableCollection<Workspace> Workspaces { get; set; } = [];

    [ObservableProperty]
    public partial Workspace? SelectedWorkspace { get; set; }

    [ObservableProperty]
    public partial long TotalFileCount { get; set; }

    [ObservableProperty]
    public partial string TotalStorageFormatted { get; set; } = "0 B";

    [ObservableProperty]
    public partial bool HasFiles { get; set; } = true;

    [ObservableProperty]
    public partial ObservableCollection<StatisticCardViewModel> StatCards { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<CategoryCardViewModel> CategoryCards { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<FileItem> RecentFiles { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<FileItem> LargestFiles { get; set; } = [];

    [ObservableProperty]
    public partial StorageAnalyticsViewModel StorageAnalytics { get; set; } = new();

    [ObservableProperty]
    public partial DuplicateSummaryViewModel DuplicateSummary { get; set; } = new();

    public DashboardViewModel()
    {
        // Design-Time Constructor with mock visual data
        SetGreeting();
        LoadDesignTimeData();
    }

    public DashboardViewModel(IWorkspaceService workspaceService, IFileService fileService, Action<string>? onNavigateSection = null)
    {
        _workspaceService = workspaceService;
        _fileService = fileService;
        _onNavigateSection = onNavigateSection;

        SetGreeting();
        _ = RefreshDashboardAsync();
    }

    private void SetGreeting()
    {
        int hour = DateTime.Now.Hour;
        if (hour < 12)
            GreetingText = "Good Morning 👋";
        else if (hour < 18)
            GreetingText = "Good Afternoon ☀️";
        else
            GreetingText = "Good Evening 🌙";
    }

    public async Task RefreshDashboardAsync(string? workspaceId = null)
    {
        if (_workspaceService == null || _fileService == null)
        {
            LoadDesignTimeData();
            return;
        }

        // Fetch Workspaces
        var wsList = await _workspaceService.GetWorkspacesAsync();
        Workspaces = new ObservableCollection<Workspace>(wsList);
        if (SelectedWorkspace == null && Workspaces.Any())
        {
            SelectedWorkspace = Workspaces.First();
        }

        string? currentWsId = workspaceId ?? SelectedWorkspace?.Id;

        // Fetch total file count
        TotalFileCount = await _fileService.GetTotalFilesCountAsync(currentWsId);
        HasFiles = TotalFileCount > 0;

        // Fetch files for statistics and collections
        var allFiles = await _fileService.QueryFilesAsync(new FileSearchQuery
        {
            WorkspaceId = currentWsId,
            Limit = 5000
        });

        long totalBytes = allFiles.Sum(f => f.Size);
        TotalStorageFormatted = FormatSize(totalBytes);

        var catCounts = await _fileService.GetCategoryCountsAsync(currentWsId);
        var extCounts = await _fileService.GetExtensionCountsAsync(currentWsId);

        // Calculate statistics cards
        int categoryCount = catCounts.Count(c => c.Value > 0);
        int extensionCount = extCounts.Count;
        var duplicateFiles = allFiles.Where(f => !string.IsNullOrEmpty(f.FileHash))
            .GroupBy(f => f.FileHash)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Skip(1))
            .ToList();

        long duplicateBytes = duplicateFiles.Sum(f => f.Size);
        int indexedFoldersCount = allFiles.Select(f => Path.GetDirectoryName(f.AbsolutePath)).Distinct().Count();
        int recentChangesCount = allFiles.Count(f => f.ModifiedAt >= DateTime.Now.AddDays(-7));

        StatCards =
        [
            new StatisticCardViewModel { Title = "Indexed Files", Value = TotalFileCount.ToString("N0"), Subtitle = "Total indexed", Icon = "📚", BadgeColor = "#6366F1", TrendText = "⚡ Verified", IsPositiveTrend = true },
            new StatisticCardViewModel { Title = "Indexed Storage", Value = TotalStorageFormatted, Subtitle = "Disk space indexed", Icon = "💾", BadgeColor = "#3B82F6", TrendText = "📊 Active", IsPositiveTrend = true },
            new StatisticCardViewModel { Title = "Categories", Value = categoryCount.ToString(), Subtitle = "Active file types", Icon = "🏷️", BadgeColor = "#10B981", TrendText = $"{categoryCount} categories", IsPositiveTrend = true },
            new StatisticCardViewModel { Title = "Extensions", Value = extensionCount.ToString(), Subtitle = "Unique formats", Icon = "🧩", BadgeColor = "#06B6D4", TrendText = "Formats mapped", IsPositiveTrend = true },
            new StatisticCardViewModel { Title = "Duplicates", Value = duplicateFiles.Count.ToString(), Subtitle = FormatSize(duplicateBytes) + " duplicate", Icon = "🗑️", BadgeColor = "#EF4444", TrendText = "Savings potential", IsPositiveTrend = false },
            new StatisticCardViewModel { Title = "Indexed Folders", Value = indexedFoldersCount.ToString(), Subtitle = "Root locations", Icon = "📁", BadgeColor = "#8B5CF6", TrendText = "Directories scanned", IsPositiveTrend = true },
            new StatisticCardViewModel { Title = "Recent Changes", Value = recentChangesCount.ToString(), Subtitle = "Updated this week", Icon = "🕒", BadgeColor = "#F59E0B", TrendText = "7 days active", IsPositiveTrend = true }
        ];

        // Category Cards
        var categoryMetadata = new (FileCategory Cat, string Name, string Icon, string Color)[]
        {
            (FileCategory.Documents, "Documents", "📄", "#3B82F6"),
            (FileCategory.Images, "Images", "🖼️", "#10B981"),
            (FileCategory.Videos, "Videos", "🎥", "#F59E0B"),
            (FileCategory.Audio, "Audio", "🎵", "#8B5CF6"),
            (FileCategory.Code, "Programming", "💻", "#06B6D4"),
            (FileCategory.Archives, "Archives", "📦", "#64748B"),
            (FileCategory.Executables, "Executables", "⚙️", "#EF4444"),
            (FileCategory.Other, "Folders & Other", "📁", "#94A3B8")
        };

        var catCardList = new List<CategoryCardViewModel>();
        foreach (var (cat, name, icon, color) in categoryMetadata)
        {
            long count = catCounts.TryGetValue(cat, out var c) ? c : 0;
            var catFiles = allFiles.Where(f => f.Category == cat).ToList();
            long catBytes = catFiles.Sum(f => f.Size);
            double pct = totalBytes > 0 ? (double)catBytes / totalBytes * 100 : 0;

            catCardList.Add(new CategoryCardViewModel
            {
                Category = cat,
                Name = name,
                Icon = icon,
                FileCount = count,
                StorageBytes = catBytes,
                FormattedStorage = FormatSize(catBytes),
                Percentage = Math.Round(pct, 1),
                BadgeColor = color
            });
        }
        CategoryCards = new ObservableCollection<CategoryCardViewModel>(catCardList);

        // Recent Files (Top 6 by ModifiedAt)
        RecentFiles = new ObservableCollection<FileItem>(allFiles.OrderByDescending(f => f.ModifiedAt).Take(6));

        // Largest Files (Top 10 by Size)
        LargestFiles = new ObservableCollection<FileItem>(allFiles.OrderByDescending(f => f.Size).Take(10));

        // Storage Analytics
        var storageVM = new StorageAnalyticsViewModel
        {
            TotalStorageFormatted = TotalStorageFormatted
        };

        foreach (var catCard in catCardList.OrderByDescending(c => c.StorageBytes).Take(5))
        {
            storageVM.Categories.Add(new CategoryStorageItem
            {
                Name = catCard.Name,
                Icon = catCard.Icon,
                FormattedSize = catCard.FormattedStorage,
                Percentage = catCard.Percentage,
                Color = catCard.BadgeColor
            });
        }

        int extColorIdx = 0;
        string[] palette = { "#6366F1", "#10B981", "#F59E0B", "#38BDF8", "#EC4899", "#8B5CF6" };
        foreach (var ext in extCounts.OrderByDescending(e => e.Value).Take(6))
        {
            long extBytes = allFiles.Where(f => f.Extension.Equals(ext.Key, StringComparison.OrdinalIgnoreCase)).Sum(f => f.Size);
            double pct = totalBytes > 0 ? (double)extBytes / totalBytes * 100 : 0;
            storageVM.TopExtensions.Add(new ExtensionStorageItem
            {
                Extension = string.IsNullOrEmpty(ext.Key) ? "Unknown" : $".{ext.Key.TrimStart('.')}",
                Count = ext.Value,
                FormattedSize = FormatSize(extBytes),
                Percentage = Math.Round(pct, 1),
                Color = palette[extColorIdx % palette.Length]
            });
            extColorIdx++;
        }

        // Largest Folders
        var folderGroups = allFiles.GroupBy(f => Path.GetDirectoryName(f.AbsolutePath) ?? "Root")
            .Select(g => new
            {
                Path = g.Key,
                Count = g.Count(),
                Size = g.Sum(f => f.Size)
            })
            .OrderByDescending(g => g.Size)
            .Take(5);

        foreach (var fg in folderGroups)
        {
            double pct = totalBytes > 0 ? (double)fg.Size / totalBytes * 100 : 0;
            storageVM.LargestFolders.Add(new FolderStorageItem
            {
                FolderPath = fg.Path,
                FolderName = Path.GetFileName(fg.Path) is string n && !string.IsNullOrEmpty(n) ? n : fg.Path,
                FileCount = fg.Count,
                FormattedSize = FormatSize(fg.Size),
                Percentage = Math.Round(pct, 1)
            });
        }
        StorageAnalytics = storageVM;

        // Duplicate Summary
        DuplicateSummary = new DuplicateSummaryViewModel
        {
            GroupCount = allFiles.Where(f => !string.IsNullOrEmpty(f.FileHash)).GroupBy(f => f.FileHash).Count(g => g.Count() > 1),
            TotalDuplicateCount = duplicateFiles.Count,
            DuplicateStorageFormatted = FormatSize(duplicateBytes),
            PotentialSavingsFormatted = FormatSize(duplicateBytes)
        };
    }

    private void LoadDesignTimeData()
    {
        HasFiles = true;
        TotalFileCount = 14820;
        TotalStorageFormatted = "184.2 GB";

        StatCards =
        [
            new StatisticCardViewModel { Title = "Indexed Files", Value = "14,820", Subtitle = "Total indexed", Icon = "📚", BadgeColor = "#6366F1", TrendText = "⚡ Verified", IsPositiveTrend = true },
            new StatisticCardViewModel { Title = "Indexed Storage", Value = "184.2 GB", Subtitle = "Disk space indexed", Icon = "💾", BadgeColor = "#3B82F6", TrendText = "📊 Active", IsPositiveTrend = true },
            new StatisticCardViewModel { Title = "Categories", Value = "8", Subtitle = "Active file types", Icon = "🏷️", BadgeColor = "#10B981", TrendText = "8 categories", IsPositiveTrend = true },
            new StatisticCardViewModel { Title = "Extensions", Value = "42", Subtitle = "Unique formats", Icon = "🧩", BadgeColor = "#06B6D4", TrendText = "Formats mapped", IsPositiveTrend = true },
            new StatisticCardViewModel { Title = "Duplicates", Value = "128", Subtitle = "1.4 GB duplicate", Icon = "🗑️", BadgeColor = "#EF4444", TrendText = "Savings potential", IsPositiveTrend = false },
            new StatisticCardViewModel { Title = "Indexed Folders", Value = "14", Subtitle = "Root locations", Icon = "📁", BadgeColor = "#8B5CF6", TrendText = "Directories scanned", IsPositiveTrend = true },
            new StatisticCardViewModel { Title = "Recent Changes", Value = "254", Subtitle = "Updated this week", Icon = "🕒", BadgeColor = "#F59E0B", TrendText = "7 days active", IsPositiveTrend = true }
        ];

        CategoryCards =
        [
            new CategoryCardViewModel { Category = FileCategory.Documents, Name = "Documents", Icon = "📄", FileCount = 3412, FormattedStorage = "12.4 GB", Percentage = 18.5, BadgeColor = "#3B82F6" },
            new CategoryCardViewModel { Category = FileCategory.Images, Name = "Images", Icon = "🖼️", FileCount = 6840, FormattedStorage = "42.8 GB", Percentage = 32.1, BadgeColor = "#10B981" },
            new CategoryCardViewModel { Category = FileCategory.Videos, Name = "Videos", Icon = "🎥", FileCount = 412, FormattedStorage = "84.6 GB", Percentage = 45.2, BadgeColor = "#F59E0B" },
            new CategoryCardViewModel { Category = FileCategory.Audio, Name = "Audio", Icon = "🎵", FileCount = 1250, FormattedStorage = "14.2 GB", Percentage = 8.4, BadgeColor = "#8B5CF6" },
            new CategoryCardViewModel { Category = FileCategory.Code, Name = "Programming", Icon = "💻", FileCount = 2100, FormattedStorage = "3.1 GB", Percentage = 2.2, BadgeColor = "#06B6D4" },
            new CategoryCardViewModel { Category = FileCategory.Archives, Name = "Archives", Icon = "📦", FileCount = 180, FormattedStorage = "18.5 GB", Percentage = 10.1, BadgeColor = "#64748B" },
            new CategoryCardViewModel { Category = FileCategory.Executables, Name = "Executables", Icon = "⚙️", FileCount = 95, FormattedStorage = "6.4 GB", Percentage = 3.5, BadgeColor = "#EF4444" },
            new CategoryCardViewModel { Category = FileCategory.Other, Name = "Folders & Other", Icon = "📁", FileCount = 531, FormattedStorage = "2.2 GB", Percentage = 1.2, BadgeColor = "#94A3B8" }
        ];

        RecentFiles =
        [
            new FileItem { Name = "Quarterly_Report_2026.pdf", Extension = "pdf", Category = FileCategory.Documents, AbsolutePath = "/home/user/Documents/Quarterly_Report_2026.pdf", Size = 4520100, ModifiedAt = DateTime.Now.AddHours(-2) },
            new FileItem { Name = "Project_Architecture.png", Extension = "png", Category = FileCategory.Images, AbsolutePath = "/home/user/Pictures/Project_Architecture.png", Size = 12400000, ModifiedAt = DateTime.Now.AddHours(-5) },
            new FileItem { Name = "System_Demo.mp4", Extension = "mp4", Category = FileCategory.Videos, AbsolutePath = "/home/user/Videos/System_Demo.mp4", Size = 450000000, ModifiedAt = DateTime.Now.AddDays(-1) },
            new FileItem { Name = "main_engine.rs", Extension = "rs", Category = FileCategory.Code, AbsolutePath = "/home/user/Projects/FileNexus/native/main.rs", Size = 45000, ModifiedAt = DateTime.Now.AddHours(-1) }
        ];

        LargestFiles =
        [
            new FileItem { Name = "Ubuntu_24_04_LTS.iso", Extension = "iso", Category = FileCategory.Archives, AbsolutePath = "/home/user/Downloads/Ubuntu_24_04_LTS.iso", Size = 5400000000, ModifiedAt = DateTime.Now.AddDays(-12) },
            new FileItem { Name = "Dataset_Backup_2026.zip", Extension = "zip", Category = FileCategory.Archives, AbsolutePath = "/home/user/Backups/Dataset_Backup_2026.zip", Size = 3800000000, ModifiedAt = DateTime.Now.AddDays(-3) },
            new FileItem { Name = "Presentation_4K.mov", Extension = "mov", Category = FileCategory.Videos, AbsolutePath = "/home/user/Videos/Presentation_4K.mov", Size = 2400000000, ModifiedAt = DateTime.Now.AddDays(-5) }
        ];

        var storageVM = new StorageAnalyticsViewModel
        {
            TotalStorageFormatted = "184.2 GB",
            Categories =
            [
                new CategoryStorageItem { Name = "Videos", Icon = "🎥", FormattedSize = "84.6 GB", Percentage = 45.9, Color = "#F59E0B" },
                new CategoryStorageItem { Name = "Images", Icon = "🖼️", FormattedSize = "42.8 GB", Percentage = 23.2, Color = "#10B981" },
                new CategoryStorageItem { Name = "Archives", Icon = "📦", FormattedSize = "18.5 GB", Percentage = 10.0, Color = "#64748B" },
                new CategoryStorageItem { Name = "Audio", Icon = "🎵", FormattedSize = "14.2 GB", Percentage = 7.7, Color = "#8B5CF6" },
                new CategoryStorageItem { Name = "Documents", Icon = "📄", FormattedSize = "12.4 GB", Percentage = 6.7, Color = "#3B82F6" }
            ],
            TopExtensions =
            [
                new ExtensionStorageItem { Extension = ".mp4", Count = 312, FormattedSize = "62.4 GB", Percentage = 33.8, Color = "#F59E0B" },
                new ExtensionStorageItem { Extension = ".png", Count = 4820, FormattedSize = "28.1 GB", Percentage = 15.2, Color = "#10B981" },
                new ExtensionStorageItem { Extension = ".iso", Count = 4, FormattedSize = "18.2 GB", Percentage = 9.8, Color = "#EC4899" },
                new ExtensionStorageItem { Extension = ".pdf", Count = 1840, FormattedSize = "10.4 GB", Percentage = 5.6, Color = "#3B82F6" },
                new ExtensionStorageItem { Extension = ".zip", Count = 140, FormattedSize = "9.8 GB", Percentage = 5.3, Color = "#64748B" }
            ],
            LargestFolders =
            [
                new FolderStorageItem { FolderPath = "/home/user/Videos", FolderName = "Videos", FileCount = 412, FormattedSize = "84.6 GB", Percentage = 45.9 },
                new FolderStorageItem { FolderPath = "/home/user/Pictures", FolderName = "Pictures", FileCount = 6840, FormattedSize = "42.8 GB", Percentage = 23.2 },
                new FolderStorageItem { FolderPath = "/home/user/Downloads", FolderName = "Downloads", FileCount = 1520, FormattedSize = "26.4 GB", Percentage = 14.3 }
            ]
        };
        StorageAnalytics = storageVM;

        DuplicateSummary = new DuplicateSummaryViewModel
        {
            GroupCount = 24,
            TotalDuplicateCount = 128,
            DuplicateStorageFormatted = "1.4 GB",
            PotentialSavingsFormatted = "1.4 GB"
        };
    }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    [RelayCommand]
    private void SelectCategoryCard(CategoryCardViewModel? categoryCard)
    {
        if (categoryCard == null) return;
        _onNavigateSection?.Invoke(categoryCard.Name);
    }

    [RelayCommand]
    private void OpenFile(FileItem? file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.AbsolutePath)) return;
        if (File.Exists(file.AbsolutePath))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = file.AbsolutePath,
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }

    [RelayCommand]
    private void RevealFile(FileItem? file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.AbsolutePath)) return;
        string? dir = Path.GetDirectoryName(file.AbsolutePath);
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }

    [RelayCommand]
    private async Task ToggleFavorite(FileItem? file)
    {
        if (file == null || _fileService == null) return;
        file.IsFavorite = !file.IsFavorite;
        await _fileService.ToggleFavoriteAsync(file.Id, file.IsFavorite);
    }

    [RelayCommand]
    private void OpenDuplicates()
    {
        _onNavigateSection?.Invoke("Duplicates");
    }

    [RelayCommand]
    private void ScanFolder()
    {
        _onNavigateSection?.Invoke("ScanFolder");
    }

    [RelayCommand]
    private void RefreshIndex()
    {
        _ = RefreshDashboardAsync();
    }

    [RelayCommand]
    private void NewWorkspace()
    {
        _onNavigateSection?.Invoke("NewWorkspace");
    }

    [RelayCommand]
    private void ImportIndex()
    {
        _onNavigateSection?.Invoke("Import");
    }

    [RelayCommand]
    private void Settings()
    {
        _onNavigateSection?.Invoke("Settings");
    }
}
