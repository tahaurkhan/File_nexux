using CommunityToolkit.Mvvm.ComponentModel;

namespace FileNexus.UI.ViewModels;

public partial class ExtensionItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _extension = string.Empty;

    [ObservableProperty]
    private long _count;

    [ObservableProperty]
    private bool _isSelected;
}
