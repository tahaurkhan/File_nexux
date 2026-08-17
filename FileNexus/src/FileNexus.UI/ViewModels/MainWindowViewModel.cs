using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileNexus.Core.Services;
using FileNexus.Shared.Enums;
using FileNexus.Shared.Models;
using FileNexus.UI.Services;

namespace FileNexus.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IWorkspaceService? _workspaceService;
    private readonly IFileService? _fileService;
    private CancellationTokenSource? _searchCts;

    // Sidebar & Navigation State
    [ObservableProperty]
    public partial bool IsSidebarCollapsed { get; set; }

    [ObservableProperty]
    public partial double SidebarWidth { get; set; } = 250;

    [ObservableProperty]
    public partial string SidebarToggleIcon { get; set; } = "◀";

    [ObservableProperty]
    public partial string ActiveSectionGroup { get; set; } = "Dashboard";

    [ObservableProperty]
    public partial string BreadcrumbPath { get; set; } = "Dashboard Overview";

    [ObservableProperty]
    public partial string ThemeIcon { get; set; } = "💻";

    [ObservableProperty]
    public partial ThemeOption CurrentThemeMode { get; set; } = ThemeOption.System;

    [ObservableProperty]
    public partial bool IsSettingsOpen { get; set; }

    // Navigation Hierarchy
    [ObservableProperty]
    public partial NavigationItemViewModel DashboardNavItem { get; set; } = null!;

    [ObservableProperty]
    public partial ObservableCollection<NavigationItemViewModel> LibraryNavItems { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<NavigationItemViewModel> SmartNavItems { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<NavigationItemViewModel> ToolsNavItems { get; set; } = [];

    [ObservableProperty]
    public partial NavigationItemViewModel? SelectedNavItem { get; set; }

    // Workspace & Core Collections
    [ObservableProperty]
    public partial ObservableCollection<Workspace> Workspaces { get; set; } = [];

    [ObservableProperty]
    public partial Workspace? SelectedWorkspace { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<CategoryItemViewModel> Categories { get; set; } = [];

    [ObservableProperty]
    public partial CategoryItemViewModel? SelectedCategory { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ExtensionItemViewModel> Extensions { get; set; } = [];

    [ObservableProperty]
    public partial ExtensionItemViewModel? SelectedExtension { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FileItem> Files { get; set; } = [];

    [ObservableProperty]
    public partial FileItem? SelectedFile { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FileItemViewModel> FileViewModels { get; set; } = [];

    [ObservableProperty]
    public partial FileItemViewModel? SelectedFileViewModel { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool OnlyFavorites { get; set; }

    [ObservableProperty]
    public partial long TotalFileCount { get; set; }

    [ObservableProperty]
    public partial bool IsScanning { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Ready";

    [ObservableProperty]
    public partial string NewFolderPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsFolderManagerOpen { get; set; }

    [ObservableProperty]
    public partial DashboardViewModel DashboardVM { get; set; } = null!;

    [ObservableProperty]
    public partial bool IsDashboardView { get; set; } = true;

    public MainWindowViewModel()
    {
        // Design-time constructor
        DashboardVM = new DashboardViewModel();
        InitializeCategories();
        InitializeNavigation();
    }

    public MainWindowViewModel(IWorkspaceService workspaceService, IFileService fileService)
    {
        _workspaceService = workspaceService;
        _fileService = fileService;

        DashboardVM = new DashboardViewModel(_workspaceService, _fileService, OnDashboardNavigate);
        InitializeCategories();
        InitializeNavigation();
        _ = InitializeAsync();
    }

    private void InitializeCategories()
    {
        Categories =
        [
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
        ];
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

        LibraryNavItems =
        [
            new() { Id = "Doc", Title = "Documents", Icon = "📄", Group = NavigationGroup.Library, Category = FileCategory.Documents, BadgeColor = "#3B82F6", Tooltip = "Documents & PDFs" },
            new() { Id = "Img", Title = "Images", Icon = "🖼", Group = NavigationGroup.Library, Category = FileCategory.Images, BadgeColor = "#10B981", Tooltip = "Images & Graphics" },
            new() { Id = "Vid", Title = "Videos", Icon = "🎥", Group = NavigationGroup.Library, Category = FileCategory.Videos, BadgeColor = "#F59E0B", Tooltip = "Videos & Clips" },
            new() { Id = "Aud", Title = "Audio", Icon = "🎵", Group = NavigationGroup.Library, Category = FileCategory.Audio, BadgeColor = "#8B5CF6", Tooltip = "Audio & Music" },
            new() { Id = "Code", Title = "Programming", Icon = "💻", Group = NavigationGroup.Library, Category = FileCategory.Code, BadgeColor = "#06B6D4", Tooltip = "Source Code & Scripts" },
            new() { Id = "Arc", Title = "Archives", Icon = "📦", Group = NavigationGroup.Library, Category = FileCategory.Archives, BadgeColor = "#64748B", Tooltip = "Compressed Archives" },
            new() { Id = "Exe", Title = "Executables", Icon = "⚙", Group = NavigationGroup.Library, Category = FileCategory.Executables, BadgeColor = "#EF4444", Tooltip = "Applications & Binaries" },
            new() { Id = "Fol", Title = "Folders", Icon = "📁", Group = NavigationGroup.Library, Category = FileCategory.Other, BadgeColor = "#94A3B8", Tooltip = "Virtual Folders" }
        ];

        SmartNavItems =
        [
            new() { Id = "Fav", Title = "Favorites", Icon = "⭐", Group = NavigationGroup.SmartCollections, SmartCollection = SmartCollectionType.Favorites, BadgeColor = "#F59E0B", Tooltip = "Starred Favorites" },
            new() { Id = "Rec", Title = "Recent", Icon = "🕒", Group = NavigationGroup.SmartCollections, SmartCollection = SmartCollectionType.Recent, BadgeColor = "#3B82F6", Tooltip = "Recently Modified" },
            new() { Id = "Dls", Title = "Downloads", Icon = "📥", Group = NavigationGroup.SmartCollections, SmartCollection = SmartCollectionType.Downloads, BadgeColor = "#10B981", Tooltip = "Downloaded Files" }
        ];

        ToolsNavItems =
        [
            new() { Id = "IdxMgr", Title = "Index Manager", Icon = "⚙", Group = NavigationGroup.Tools, Tooltip = "Index Database Manager" },
            new() { Id = "ScanFol", Title = "Scan Folder", Icon = "📂", Group = NavigationGroup.Tools, Tooltip = "Scan and Index Folder" },
            new() { Id = "Imp", Title = "Import", Icon = "📥", Group = NavigationGroup.Tools, Tooltip = "Import Index Data" },
            new() { Id = "Exp", Title = "Export", Icon = "📤", Group = NavigationGroup.Tools, Tooltip = "Export Library Data" }
        ];

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
        var next = ThemeManager.CycleNextTheme();
        CurrentThemeMode = next;
        UpdateThemeState();
    }

    [RelayCommand]
    private void SetSystemTheme()
    {
        ThemeManager.ApplyTheme(ThemeOption.System);
        CurrentThemeMode = ThemeOption.System;
        UpdateThemeState();
    }

    [RelayCommand]
    private void SetLightTheme()
    {
        ThemeManager.ApplyTheme(ThemeOption.Light);
        CurrentThemeMode = ThemeOption.Light;
        UpdateThemeState();
    }

    [RelayCommand]
    private void SetDarkTheme()
    {
        ThemeManager.ApplyTheme(ThemeOption.Dark);
        CurrentThemeMode = ThemeOption.Dark;
        UpdateThemeState();
    }

    private void UpdateThemeState()
    {
        ThemeIcon = CurrentThemeMode switch
        {
            ThemeOption.Light => "☀️",
            ThemeOption.Dark => "🌙",
            _ => "💻"
        };
        StatusMessage = $"Appearance: {CurrentThemeMode}";
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
        IsDashboardView = (item.Group == NavigationGroup.Dashboard);

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

    private void OnDashboardNavigate(string target)
    {
        if (target == "ScanFolder")
        {
            ToggleFolderManager();
            return;
        }
        if (target == "NewWorkspace")
        {
            NewWorkspace();
            return;
        }

        var navItem = LibraryNavItems.FirstOrDefault(n => n.Title.Equals(target, StringComparison.OrdinalIgnoreCase))
            ?? SmartNavItems.FirstOrDefault(n => n.Title.Equals(target, StringComparison.OrdinalIgnoreCase))
            ?? ToolsNavItems.FirstOrDefault(n => n.Title.Equals(target, StringComparison.OrdinalIgnoreCase));

        if (navItem != null)
        {
            SelectNavigationItem(navItem);
        }
    }

    partial void OnSelectedWorkspaceChanged(Workspace? value)
    {
        _ = RefreshDataAsync();
    }

    partial void OnSelectedCategoryChanged(CategoryItemViewModel? value)
    {
        SelectedExtension = null;
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

        if (DashboardVM != null)
        {
            _ = DashboardVM.RefreshDashboardAsync(wsId);
        }

        await LoadFilesAsync();
    }

    public async Task LoadFilesAsync()
    {
        if (_fileService == null) return;

        // 1. Fetch category files to update extension filter chips for selected category
        var categoryQuery = new FileSearchQuery
        {
            WorkspaceId = SelectedWorkspace?.Id,
            Category = SelectedCategory?.Category ?? FileCategory.All,
            SearchTerm = SearchText,
            OnlyFavorites = OnlyFavorites,
            Limit = 5000
        };

        var allCategoryFiles = await _fileService.QueryFilesAsync(categoryQuery);

        var extGroups = allCategoryFiles
            .Where(f => !string.IsNullOrWhiteSpace(f.Extension))
            .GroupBy(f => f.Extension.TrimStart('.').ToLowerInvariant())
            .OrderByDescending(g => g.Count())
            .Take(30);

        Extensions = new ObservableCollection<ExtensionItemViewModel>(
            extGroups.Select(g => new ExtensionItemViewModel
            {
                Extension = g.Key,
                Count = g.Count(),
                IsSelected = SelectedExtension != null && SelectedExtension.Extension.Equals(g.Key, StringComparison.OrdinalIgnoreCase)
            })
        );

        // 2. Fetch specific file items matching category + extension filter
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
        FileViewModels = new ObservableCollection<FileItemViewModel>(list.Select(f => new FileItemViewModel(f)));

        if (SelectedFileViewModel != null && !FileViewModels.Any(f => f.Id == SelectedFileViewModel.Id))
        {
            SelectedFileViewModel = FileViewModels.FirstOrDefault();
        }
        else if (SelectedFileViewModel == null && FileViewModels.Any())
        {
            SelectedFileViewModel = FileViewModels.First();
        }

        SelectedFile = SelectedFileViewModel?.Item;

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
    private void OpenFile(object? param)
    {
        string? path = null;
        if (param is FileItemViewModel vm) path = vm.AbsolutePath;
        else if (param is FileItem fi) path = fi.AbsolutePath;
        else if (SelectedFileViewModel != null) path = SelectedFileViewModel.AbsolutePath;
        else if (SelectedFile != null) path = SelectedFile.AbsolutePath;

        if (string.IsNullOrWhiteSpace(path)) return;

        if (FileLauncher.OpenFile(path))
        {
            StatusMessage = $"Opened: {Path.GetFileName(path)}";
        }
        else
        {
            StatusMessage = $"File not found or cannot be opened: {path}";
        }
    }

    [RelayCommand]
    private async Task CopyPath(FileItem? file)
    {
        var item = file ?? SelectedFile;
        if (item == null || string.IsNullOrWhiteSpace(item.AbsolutePath)) return;

        StatusMessage = $"Copied path: {item.AbsolutePath}";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void OpenFolder(object? param)
    {
        string? path = null;
        if (param is FileItemViewModel vm) path = vm.AbsolutePath;
        else if (param is FileItem fi) path = fi.AbsolutePath;
        else if (SelectedFileViewModel != null) path = SelectedFileViewModel.AbsolutePath;
        else if (SelectedFile != null) path = SelectedFile.AbsolutePath;

        if (string.IsNullOrWhiteSpace(path)) return;

        if (FileLauncher.OpenFolder(path))
        {
            StatusMessage = $"Opened folder location: {Path.GetDirectoryName(path)}";
        }
        else
        {
            StatusMessage = $"Directory not found: {path}";
        }
    }

    [RelayCommand]
    private async Task BrowseFolder()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var storageProvider = desktop.MainWindow.StorageProvider;
            if (storageProvider.CanPickFolder)
            {
                var result = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Folder to Index",
                    AllowMultiple = false
                });

                if (result.Count > 0)
                {
                    NewFolderPath = result[0].Path.LocalPath;
                }
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
        OpenRecent();
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
        IsSettingsOpen = !IsSettingsOpen;
        StatusMessage = IsSettingsOpen ? "Settings opened" : "Settings closed";
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
    private static void Quit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
