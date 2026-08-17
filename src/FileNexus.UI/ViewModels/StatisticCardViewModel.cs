using CommunityToolkit.Mvvm.ComponentModel;

namespace FileNexus.UI.ViewModels;

public partial class StatisticCardViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Value { get; set; } = "0";

    [ObservableProperty]
    public partial string Subtitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Icon { get; set; } = "📊";

    [ObservableProperty]
    public partial string BadgeColor { get; set; } = "#6366F1";

    [ObservableProperty]
    public partial string TrendText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsPositiveTrend { get; set; } = true;
}
