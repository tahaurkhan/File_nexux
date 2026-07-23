using System.Collections.Generic;
using System.Threading.Tasks;
using FileNexus.Shared.Enums;
using FileNexus.Shared.Models;

namespace FileNexus.Database.Repositories;

public interface IFileRecordRepository
{
    Task BulkUpsertBatchAsync(IEnumerable<FileItem> items);
    Task DeleteByFolderIdAsync(string folderId);
    Task DeleteByWorkspaceIdAsync(string workspaceId);
    Task<List<FileItem>> SearchAsync(FileSearchQuery query);
    Task<long> GetTotalCountAsync(string? workspaceId = null);
    Task<Dictionary<FileCategory, long>> GetCategoryCountsAsync(string? workspaceId = null);
    Task<Dictionary<string, long>> GetExtensionCountsAsync(string? workspaceId = null);
    Task ToggleFavoriteAsync(string fileId, bool isFavorite);
    Task UpdateTagsAsync(string fileId, string tags);
    Task<FileItem?> GetByIdAsync(string id);
}
