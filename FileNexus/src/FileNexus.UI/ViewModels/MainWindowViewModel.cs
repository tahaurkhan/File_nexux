using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileNexus.Core.Services;
using FileNexus.Shared.Enums;
using FileNexus.Shared.Models;

namespace FileNexus.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IWorkspaceService? _workspaceService;
    private readonly IFileService? _fileService;
    private CancellationTokenSource? _searchCts;

    [ObservableProperty]
    private ObservableCollection<Workspace> _workspaces = new();

    [ObservableProperty]
    private Workspace? _selectedWorkspace;

    [ObservableProperty]
    private ObservableCollection<CategoryItemViewModel> _categories = new();

    [ObservableProperty]
    private CategoryItemViewModel? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<ExtensionItemViewModel> _extensions = new();

    [ObservableProperty]
    private ExtensionItemViewModel? _selectedExtension;

    [ObservableProperty]
    private ObservableCollection<FileItem> _files = new();

    [ObservableProperty]
    private FileItem? _selectedFile;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _onlyFavorites;

    [ObservableProperty]
    private long _totalFileCount;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _newFolderPath = string.Empty;

    [ObservableProperty]
    private bool _isFolderManagerOpen;

    public MainWindowViewModel()
    {
        // Design-time constructor
        InitializeCategories();
    }

    public MainWindowViewModel(IWorkspaceService workspaceService, IFileService fileService)
    {
        _workspaceService = workspaceService;
        _fileService = fileService;

        InitializeCategories();
        _ = InitializeAsync();
    }

    private void InitializeCategories()
    {
        Categories = new ObservableCollection<CategoryItemViewModel>
        {
            new() { Category = FileCategory.All, Name = "All Files", Icon = "📚", BadgeColor = "#6366F1", IsSelected = true },
            new() { Category = FileCategory.Books, Name = "Books & PDFs", Icon = "📖", BadgeColor = "#EC4899" },
            new() { Category = FileCategory.Documents, Name = "Documents", Icon = "📄", BadgeColor = "#3B82F6" },
            new() { Category = FileCategory.Images, Name = "Images", Icon = "🖼️", BadgeColor = "#10B981" },
            new() { Category = FileCategory.Videos, Name = "Videos", Icon = "🎥", BadgeColor = "#F59E0B" },
            new() { Category = FileCategory.Audio, Name = "Audio", Icon = "🎵", BadgeColor = "#8B5CF6" },
            new() { Category = FileCategory.Code, Name = "Source Code", Icon = "💻", BadgeColor = "#06B6D4" },
            new() { Category = FileCategory.Archives, Name = "Archives", Icon = "📦", BadgeColor = "#64748B" },
            new() { Category = FileCategory.Executables, Name = "Executables", Icon = "⚙️", BadgeColor = "#EF4444" },
            new() { Category = FileCategory.Other, Name = "Other Files", Icon = "📁", BadgeColor = "#94A3B8" }
        };
        SelectedCategory = Categories.First();
    }

    public async Task InitializeAsync()
    {
        if (_workspaceService == null || _fileService == null) return;

        StatusMessage = "Loading Workspaces...";
        var list = await _workspaceService.GetWorkspacesAsync();
        Workspaces = new ObservableCollection<Workspace>(list);
        SelectedWorkspace = Workspaces.FirstOrDefault();

        await RefreshDataAsync();
        StatusMessage = "Virtual Library Ready";
    }

    partial void OnSelectedWorkspaceChanged(Workspace? value)
    {
        _ = RefreshDataAsync();
    }

    partial void OnSelectedCategoryChanged(CategoryItemViewModel? value)
    {
        if (value != null)
        {
            foreach (var cat in Categories)
            {
                cat.IsSelected = (cat == value);
            }
        }
        _ = LoadFilesAsync();
    }

    partial void OnSelectedExtensionChanged(ExtensionItemViewModel? value)
    {
        _ = LoadFilesAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        Task.Delay(250, token).ContinueWith(t =>
        {
            if (!token.IsCancellationRequested)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => _ = LoadFilesAsync());
            }
        }, TaskScheduler.Default);
    }

    partial void OnOnlyFavoritesChanged(bool value)
    {
        _ = LoadFilesAsync();
    }

    public async Task RefreshDataAsync()
    {
        if (_fileService == null) return;

        string? wsId = SelectedWorkspace?.Id;
        TotalFileCount = await _fileService.GetTotalFilesCountAsync(wsId);

        // Update category counts
        var catCounts = await _fileService.GetCategoryCountsAsync(wsId);
        long totalSum = catCounts.Values.Sum();

        foreach (var catVM in Categories)
        {
            if (catVM.Category == FileCategory.All)
            {
                catVM.Count = totalSum;
            }
            else if (catCounts.TryGetValue(catVM.Category, out var cnt))
            {
                catVM.Count = cnt;
            }
            else
            {
                catVM.Count = 0;
            }
        }

        // Update extension chips
        var extCounts = await _fileService.GetExtensionCountsAsync(wsId);
        Extensions = new ObservableCollection<ExtensionItemViewModel>(
            extCounts.Select(kvp => new ExtensionItemViewModel { Extension = kvp.Key, Count = kvp.Value })
        );

        await LoadFilesAsync();
    }

    public async Task LoadFilesAsync()
    {
        if (_fileService == null) return;

        var query = new FileSearchQuery
        {
            WorkspaceId = SelectedWorkspace?.Id,
            Category = SelectedCategory?.Category ?? FileCategory.All,
            Extension = SelectedExtension?.Extension,
            SearchTerm = SearchText,
            OnlyFavorites = OnlyFavorites,
            Limit = 1000
        };

        var list = await _fileService.QueryFilesAsync(query);
        Files = new ObservableCollection<FileItem>(list);
    }

    [RelayCommand]
    private void SelectCategory(CategoryItemViewModel categoryVM)
    {
        SelectedCategory = categoryVM;
    }

    [RelayCommand]
    private void SelectExtension(ExtensionItemViewModel? extVM)
    {
        if (SelectedExtension == extVM)
        {
            SelectedExtension = null; // Toggle unselect
        }
        else
        {
            SelectedExtension = extVM;
        }
    }

    [RelayCommand]
    private async Task ToggleFavorite(FileItem? file)
    {
        if (file == null || _fileService == null) return;

        file.IsFavorite = !file.IsFavorite;
        await _fileService.ToggleFavoriteAsync(file.Id, file.IsFavorite);
        if (OnlyFavorites)
        {
            await LoadFilesAsync();
        }
    }

    [RelayCommand]
    private async Task SaveTags(string tags)
    {
        if (SelectedFile == null || _fileService == null) return;
        SelectedFile.Tags = tags;
        await _fileService.UpdateTagsAsync(SelectedFile.Id, tags);
    }

    [RelayCommand]
    private void OpenFile(FileItem? file)
    {
        var item = file ?? SelectedFile;
        if (item == null || string.IsNullOrWhiteSpace(item.AbsolutePath)) return;

        if (File.Exists(item.AbsolutePath))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = item.AbsolutePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Cannot open file: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void OpenFolder(FileItem? file)
    {
        var item = file ?? SelectedFile;
        if (item == null || string.IsNullOrWhiteSpace(item.AbsolutePath)) return;

        string? dir = Path.GetDirectoryName(item.AbsolutePath);
        if (Directory.Exists(dir))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Cannot open directory: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task AddWorkspaceFolder()
    {
        if (_workspaceService == null || SelectedWorkspace == null) return;
        if (string.IsNullOrWhiteSpace(NewFolderPath) || !Directory.Exists(NewFolderPath))
        {
            StatusMessage = "Please enter a valid directory path.";
            return;
        }

        IsScanning = true;
        StatusMessage = $"Indexing folder: {NewFolderPath}...";

        try
        {
            await _workspaceService.AddFolderToWorkspaceAsync(SelectedWorkspace.Id, NewFolderPath);
            NewFolderPath = string.Empty;
            IsFolderManagerOpen = false;
            await RefreshDataAsync();
            StatusMessage = "Indexing complete.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Indexing error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private async Task ScanWorkspace()
    {
        if (_workspaceService == null || SelectedWorkspace == null) return;

        IsScanning = true;
        StatusMessage = $"Re-indexing workspace '{SelectedWorkspace.Name}'...";

        try
        {
            await _workspaceService.ScanWorkspaceAsync(SelectedWorkspace.Id);
            await RefreshDataAsync();
            StatusMessage = "Re-indexing complete.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Scan error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private void ToggleFolderManager()
    {
        IsFolderManagerOpen = !IsFolderManagerOpen;
    }
}
