using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileNexus.Shared.Models;

namespace FileNexus.Core.Services;

public interface IWorkspaceService
{
    Task<List<Workspace>> GetWorkspacesAsync();
    Task<Workspace> CreateWorkspaceAsync(string name, string description, string icon = "Folder");
    Task UpdateWorkspaceAsync(Workspace workspace);
    Task DeleteWorkspaceAsync(string workspaceId);
    Task AddFolderToWorkspaceAsync(string workspaceId, string folderPath);
    Task RemoveFolderAsync(string folderId);
    Task ScanWorkspaceAsync(string workspaceId, CancellationToken cancellationToken = default);
}
