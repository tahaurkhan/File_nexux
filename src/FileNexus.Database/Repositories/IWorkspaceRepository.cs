using System.Collections.Generic;
using System.Threading.Tasks;
using FileNexus.Shared.Models;

namespace FileNexus.Database.Repositories;

public interface IWorkspaceRepository
{
    Task<List<Workspace>> GetAllAsync();
    Task<Workspace?> GetByIdAsync(string id);
    Task CreateAsync(Workspace workspace);
    Task UpdateAsync(Workspace workspace);
    Task DeleteAsync(string id);

    Task AddFolderAsync(IndexedFolder folder);
    Task DeleteFolderAsync(string folderId);
    Task UpdateFolderLastScannedAsync(string folderId, long fileCount);
}
