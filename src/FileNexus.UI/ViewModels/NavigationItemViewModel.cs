using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FileNexus.Shared.Enums;

namespace FileNexus.UI.ViewModels;

public enum NavigationGroup
{
    Dashboard,
    Library,
    SmartCollections,
    Workspaces,
    Tools,
    Footer
}

public enum SmartCollectionType
{
    None,
    Favorites,
    Duplicates,
    Recent,
    Downloads
}

public partial class NavigationItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private NavigationGroup _group;

    [ObservableProperty]
    private FileCategory? _category;

    [ObservableProperty]
    private SmartCollectionType _smartCollection = SmartCollectionType.None;

    [ObservableProperty]
    private string? _workspaceId;

    [ObservableProperty]
    private long _badgeCount;

    [ObservableProperty]
    private string _badgeColor = "#3B82F6";

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isAction;

    [ObservableProperty]
    private string _tooltip = string.Empty;
}
