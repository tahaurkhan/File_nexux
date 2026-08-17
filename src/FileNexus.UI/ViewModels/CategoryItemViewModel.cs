using CommunityToolkit.Mvvm.ComponentModel;
using FileNexus.Shared.Enums;

namespace FileNexus.UI.ViewModels;

public partial class CategoryItemViewModel : ViewModelBase
{
    public FileCategory Category { get; set; }
    
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private long _count;

    [ObservableProperty]
    private string _badgeColor = "#3B82F6";

    [ObservableProperty]
    private bool _isSelected;
}
