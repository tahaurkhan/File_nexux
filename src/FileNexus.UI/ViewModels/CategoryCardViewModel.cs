using CommunityToolkit.Mvvm.ComponentModel;
using FileNexus.Shared.Enums;

namespace FileNexus.UI.ViewModels;

public partial class CategoryCardViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial FileCategory Category { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Icon { get; set; } = "📁";

    [ObservableProperty]
    public partial long FileCount { get; set; }

    [ObservableProperty]
    public partial long StorageBytes { get; set; }

    [ObservableProperty]
    public partial string FormattedStorage { get; set; } = "0 B";

    [ObservableProperty]
    public partial double Percentage { get; set; }

    [ObservableProperty]
    public partial string BadgeColor { get; set; } = "#3B82F6";
}
