using CommunityToolkit.Mvvm.ComponentModel;

namespace FileNexus.UI.ViewModels;

public partial class DuplicateSummaryViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial int GroupCount { get; set; }

    [ObservableProperty]
    public partial int TotalDuplicateCount { get; set; }

    [ObservableProperty]
    public partial string DuplicateStorageFormatted { get; set; } = "0 B";

    [ObservableProperty]
    public partial string PotentialSavingsFormatted { get; set; } = "0 B";
}
