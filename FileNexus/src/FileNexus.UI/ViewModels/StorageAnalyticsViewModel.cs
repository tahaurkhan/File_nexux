using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileNexus.UI.ViewModels;

public partial class CategoryStorageItem : ViewModelBase
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Icon { get; set; } = "📁";

    [ObservableProperty]
    public partial string FormattedSize { get; set; } = "0 B";

    [ObservableProperty]
    public partial double Percentage { get; set; }

    [ObservableProperty]
    public partial string Color { get; set; } = "#6366F1";
}

public partial class ExtensionStorageItem : ViewModelBase
{
    [ObservableProperty]
    public partial string Extension { get; set; } = string.Empty;

    [ObservableProperty]
    public partial long Count { get; set; }

    [ObservableProperty]
    public partial string FormattedSize { get; set; } = "0 B";

    [ObservableProperty]
    public partial double Percentage { get; set; }

    [ObservableProperty]
    public partial string Color { get; set; } = "#38BDF8";
}

public partial class FolderStorageItem : ViewModelBase
{
    [ObservableProperty]
    public partial string FolderPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FolderName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial long FileCount { get; set; }

    [ObservableProperty]
    public partial string FormattedSize { get; set; } = "0 B";

    [ObservableProperty]
    public partial double Percentage { get; set; }
}

public partial class StorageAnalyticsViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string TotalStorageFormatted { get; set; } = "0 B";

    [ObservableProperty]
    public partial ObservableCollection<CategoryStorageItem> Categories { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<ExtensionStorageItem> TopExtensions { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<FolderStorageItem> LargestFolders { get; set; } = [];
}
