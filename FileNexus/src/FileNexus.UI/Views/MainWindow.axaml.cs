using Avalonia.Controls;
using Avalonia.Input;
using FileNexus.UI.ViewModels;

namespace FileNexus.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnFileDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && vm.SelectedFileViewModel != null)
        {
            vm.OpenFileCommand.Execute(vm.SelectedFileViewModel);
        }
    }
}
