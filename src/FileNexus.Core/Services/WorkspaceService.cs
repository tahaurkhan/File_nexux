using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileNexus.Database.Repositories;
using FileNexus.Shared.Models;

namespace FileNexus.Core.Services;

public sealed class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IFileRecordRepository _fileRecordRepository;
    private readonly IScannerService _scannerService;

    public WorkspaceService(
        IWorkspaceRepository workspaceRepository,
        IFileRecordRepository fileRecordRepository,
        IScannerService scannerService)
    {
        _workspaceRepository = workspaceRepository;
        _fileRecordRepository = fileRecordRepository;
        _scannerService = scannerService;
    }

    public async Task<List<Workspace>> GetWorkspacesAsync()
    {
        var workspaces = await _workspaceRepository.GetAllAsync();
        if (workspaces.Count == 0)
        {
            // Seed Default "Home" Workspace if no workspace exists yet
            var defaultWs = await CreateWorkspaceAsync("Home Workspace", "Default FileNexus Virtual Library", "Home");
            workspaces.Add(defaultWs);
        }

        return workspaces;
    }

    public async Task<Workspace> CreateWorkspaceAsync(string name, string description, string icon = "Folder")
    {
        var ws = new Workspace
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Description = description,
            Icon = icon,
            CreatedAt = DateTime.UtcNow
        };

        await _workspaceRepository.CreateAsync(ws);
        return ws;
    }

    public async Task UpdateWorkspaceAsync(Workspace workspace)
    {
        await _workspaceRepository.UpdateAsync(workspace);
    }

    public async Task DeleteWorkspaceAsync(string workspaceId)
    {
        await _fileRecordRepository.DeleteByWorkspaceIdAsync(workspaceId);
        await _workspaceRepository.DeleteAsync(workspaceId);
    }

    public async Task AddFolderToWorkspaceAsync(string workspaceId, string folderPath)
    {
        var folder = new IndexedFolder
        {
            Id = Guid.NewGuid().ToString("N"),
            WorkspaceId = workspaceId,
            Path = folderPath,
            IsActive = true
        };

        await _workspaceRepository.AddFolderAsync(folder);
        var ws = await _workspaceRepository.GetByIdAsync(workspaceId);
        if (ws != null)
        {
            await _scannerService.ScanFolderAsync(ws, folder);
        }
    }

    public async Task RemoveFolderAsync(string folderId)
    {
        await _fileRecordRepository.DeleteByFolderIdAsync(folderId);
        await _workspaceRepository.DeleteFolderAsync(folderId);
    }

    public async Task ScanWorkspaceAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        var ws = await _workspaceRepository.GetByIdAsync(workspaceId);
        if (ws == null) return;

        foreach (var folder in ws.Folders)
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (folder.IsActive)
            {
                await _scannerService.ScanFolderAsync(ws, folder, null, cancellationToken);
            }
        }
    }
}
