using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
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

    // Sidebar & Navigation State
    [ObservableProperty]
    private bool _isSidebarCollapsed;

    [ObservableProperty]
    private double _sidebarWidth = 250;

    [ObservableProperty]
    private string _sidebarToggleIcon = "◀";

    [ObservableProperty]
    private string _activeSectionGroup = "Dashboard";

    [ObservableProperty]
    private string _breadcrumbPath = "Dashboard Overview";

    [ObservableProperty]
    private bool _isDarkMode = true;

    [ObservableProperty]
    private string _themeIcon = "🌙";

    // Navigation Hierarchy
    [ObservableProperty]
    private NavigationItemViewModel _dashboardNavItem = null!;

    [ObservableProperty]
    private ObservableCollection<NavigationItemViewModel> _libraryNavItems = new();

    [ObservableProperty]
    private ObservableCollection<NavigationItemViewModel> _smartNavItems = new();

    [ObservableProperty]
    private ObservableCollection<NavigationItemViewModel> _toolsNavItems = new();

    [ObservableProperty]
    private NavigationItemViewModel? _selectedNavItem;

    // Workspace & Core Collections
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
        InitializeNavigation();
    }

    public MainWindowViewModel(IWorkspaceService workspaceService, IFileService fileService)
    {
        _workspaceService = workspaceService;
        _fileService = fileService;

        InitializeCategories();
        InitializeNavigation();
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

    private void InitializeNavigation()
    {
        DashboardNavItem = new NavigationItemViewModel
        {
            Id = "Dashboard",
            Title = "Dashboard",
            Icon = "🏠",
            Group = NavigationGroup.Dashboard,
            IsSelected = true,
            Tooltip = "Dashboard Overview"
        };

        LibraryNavItems = new ObservableCollection<NavigationItemViewModel>
        {
            new() { Id = "Doc", Title = "Documents", Icon = "📄", Group = NavigationGroup.Library, Category = FileCategory.Documents, BadgeColor = "#3B82F6", Tooltip = "Documents & PDFs" },
            new() { Id = "Img", Title = "Images", Icon = "🖼", Group = NavigationGroup.Library, Category = FileCategory.Images, BadgeColor = "#10B981", Tooltip = "Images & Graphics" },
            new() { Id = "Vid", Title = "Videos", Icon = "🎥", Group = NavigationGroup.Library, Category = FileCategory.Videos, BadgeColor = "#F59E0B", Tooltip = "Videos & Clips" },
            new() { Id = "Aud", Title = "Audio", Icon = "🎵", Group = NavigationGroup.Library, Category = FileCategory.Audio, BadgeColor = "#8B5CF6", Tooltip = "Audio & Music" },
            new() { Id = "Code", Title = "Programming", Icon = "💻", Group = NavigationGroup.Library, Category = FileCategory.Code, BadgeColor = "#06B6D4", Tooltip = "Source Code & Scripts" },
            new() { Id = "Arc", Title = "Archives", Icon = "📦", Group = NavigationGroup.Library, Category = FileCategory.Archives, BadgeColor = "#64748B", Tooltip = "Compressed Archives" },
            new() { Id = "Exe", Title = "Executables", Icon = "⚙", Group = NavigationGroup.Library, Category = FileCategory.Executables, BadgeColor = "#EF4444", Tooltip = "Applications & Binaries" },
            new() { Id = "Fol", Title = "Folders", Icon = "📁", Group = NavigationGroup.Library, Category = FileCategory.Other, BadgeColor = "#94A3B8", Tooltip = "Virtual Folders" }
        };

        SmartNavItems = new ObservableCollection<NavigationItemViewModel>
        {
            new() { Id = "Fav", Title = "Favorites", Icon = "⭐", Group = NavigationGroup.SmartCollections, SmartCollection = SmartCollectionType.Favorites, BadgeColor = "#F59E0B", Tooltip = "Starred Favorites" },
            new() { Id = "Dup", Title = "Duplicates", Icon = "🗑", Group = NavigationGroup.SmartCollections, SmartCollection = SmartCollectionType.Duplicates, BadgeColor = "#EF4444", Tooltip = "Duplicate Files" },
            new() { Id = "Rec", Title = "Recent", Icon = "🕒", Group = NavigationGroup.SmartCollections, SmartCollection = SmartCollectionType.Recent, BadgeColor = "#3B82F6", Tooltip = "Recently Modified" },
            new() { Id = "Dls", Title = "Downloads", Icon = "📥", Group = NavigationGroup.SmartCollections, SmartCollection = SmartCollectionType.Downloads, BadgeColor = "#10B981", Tooltip = "Downloaded Files" }
        };

        ToolsNavItems = new ObservableCollection<NavigationItemViewModel>
        {
            new() { Id = "IdxMgr", Title = "Index Manager", Icon = "⚙", Group = NavigationGroup.Tools, Tooltip = "Index Database Manager" },
            new() { Id = "ScanFol", Title = "Scan Folder", Icon = "📂", Group = NavigationGroup.Tools, Tooltip = "Scan and Index Folder" },
            new() { Id = "Imp", Title = "Import", Icon = "📥", Group = NavigationGroup.Tools, Tooltip = "Import Index Data" },
            new() { Id = "Exp", Title = "Export", Icon = "📤", Group = NavigationGroup.Tools, Tooltip = "Export Library Data" }
        };

        SelectedNavItem = DashboardNavItem;
    }

    public async Task InitializeAsync()
    {
        if (_workspaceService == null || _fileService == null) return;

        StatusMessage = "Loading Workspaces...";
        var list = await _workspaceService.GetWorkspacesAsync();
        Workspaces = new ObservableCollection<Workspace>(list);

        // Ensure default workspaces if none exist
        if (!Workspaces.Any())
        {
            Workspaces.Add(new Workspace { Id = "ws_personal", Name = "Personal", Description = "Personal files workspace" });
            Workspaces.Add(new Workspace { Id = "ws_college", Name = "College", Description = "College & Academic workspace" });
            Workspaces.Add(new Workspace { Id = "ws_work", Name = "Work", Description = "Professional work workspace" });
        }

        SelectedWorkspace = Workspaces.FirstOrDefault();
        await RefreshDataAsync();
        StatusMessage = "Virtual Library Ready";
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;
        SidebarWidth = IsSidebarCollapsed ? 64 : 250;
        SidebarToggleIcon = IsSidebarCollapsed ? "▶" : "◀";
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        ThemeIcon = IsDarkMode ? "🌙" : "☀️";
        StatusMessage = IsDarkMode ? "Theme: Dark" : "Theme: Light";
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    [RelayCommand]
    private void FocusSearch()
    {
        StatusMessage = "Search focused (Ctrl+K)";
    }

    [RelayCommand]
    private void SelectNavigationItem(NavigationItemViewModel? item)
    {
        if (item == null) return;

        // Reset previous selections
        DashboardNavItem.IsSelected = (item == DashboardNavItem);

        foreach (var nav in LibraryNavItems) nav.IsSelected = (nav == item);
        foreach (var nav in SmartNavItems) nav.IsSelected = (nav == item);
        foreach (var nav in ToolsNavItems) nav.IsSelected = (nav == item);

        SelectedNavItem = item;
        ActiveSectionGroup = item.Group.ToString();
        BreadcrumbPath = $"{item.Group} › {item.Title}";

        // Handle action mapping
        if (item.Group == NavigationGroup.Dashboard)
        {
            OnlyFavorites = false;
            SelectedCategory = Categories.FirstOrDefault(c => c.Category == FileCategory.All);
        }
        else if (item.Group == NavigationGroup.Library && item.Category.HasValue)
        {
            OnlyFavorites = false;
            var cat = Categories.FirstOrDefault(c => c.Category == item.Category.Value);
            if (cat != null) SelectedCategory = cat;
        }
        else if (item.Group == NavigationGroup.SmartCollections)
        {
            if (item.SmartCollection == SmartCollectionType.Favorites)
            {
                OnlyFavorites = true;
            }
            else if (item.SmartCollection == SmartCollectionType.Duplicates)
            {
                OnlyFavorites = false;
                SearchText = "duplicate:true";
            }
            else if (item.SmartCollection == SmartCollectionType.Recent)
            {
                OnlyFavorites = false;
                SearchText = "recent:true";
            }
            else if (item.SmartCollection == SmartCollectionType.Downloads)
            {
                OnlyFavorites = false;
                SearchText = "path:Downloads";
            }
        }
        else if (item.Group == NavigationGroup.Tools)
        {
            if (item.Id == "ScanFol")
            {
                ToggleFolderManager();
            }
            else
            {
                StatusMessage = $"Tool: {item.Title} selected";
            }
        }

        _ = LoadFilesAsync();
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
        foreach (var ext in Extensions)
        {
            ext.IsSelected = (ext == value);
        }
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
        DashboardNavItem.BadgeCount = TotalFileCount;

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

        // Synchronize counts with LibraryNavItems
        foreach (var libNav in LibraryNavItems)
        {
            if (libNav.Category.HasValue && catCounts.TryGetValue(libNav.Category.Value, out var count))
            {
                libNav.BadgeCount = count;
            }
            else
            {
                libNav.BadgeCount = 0;
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

        // Update Favorites badge count
        var favNav = SmartNavItems.FirstOrDefault(s => s.SmartCollection == SmartCollectionType.Favorites);
        if (favNav != null)
        {
            favNav.BadgeCount = Files.Count(f => f.IsFavorite);
        }
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

    [RelayCommand]
    private void NewWorkspace()
    {
        var newWs = new Workspace
        {
            Id = $"ws_{DateTime.UtcNow.Ticks}",
            Name = $"Workspace {Workspaces.Count + 1}",
            Description = "New Virtual Workspace"
        };
        Workspaces.Add(newWs);
        SelectedWorkspace = newWs;
        StatusMessage = $"Created new workspace: '{newWs.Name}'";
    }

    [RelayCommand]
    private void RefreshData()
    {
        _ = RefreshDataAsync();
    }

    [RelayCommand]
    private void OpenDuplicates()
    {
        var dupItem = SmartNavItems.FirstOrDefault(s => s.SmartCollection == SmartCollectionType.Duplicates);
        SelectNavigationItem(dupItem);
    }

    [RelayCommand]
    private void OpenRecent()
    {
        var recItem = SmartNavItems.FirstOrDefault(s => s.SmartCollection == SmartCollectionType.Recent);
        SelectNavigationItem(recItem);
    }

    [RelayCommand]
    private void OpenIndexManager()
    {
        StatusMessage = "Index Manager opened";
    }

    [RelayCommand]
    private void OpenSettings()
    {
        StatusMessage = "Settings view opened";
    }

    [RelayCommand]
    private void OpenAbout()
    {
        StatusMessage = "FileNexus v0.1.0 • Privacy-First Virtual File Library";
    }

    [RelayCommand]
    private void ImportIndex()
    {
        StatusMessage = "Import Index started...";
    }

    [RelayCommand]
    private void ExportIndex()
    {
        StatusMessage = "Export Index completed.";
    }

    [RelayCommand]
    private void RebuildIndex()
    {
        StatusMessage = "Rebuilding index database...";
        _ = ScanWorkspace();
    }

    [RelayCommand]
    private void Quit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
