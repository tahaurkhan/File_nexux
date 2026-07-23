using System.Collections.Generic;
using System.Threading.Tasks;
using FileNexus.Shared.Enums;
using FileNexus.Shared.Models;

namespace FileNexus.Core.Services;

public interface IFileService
{
    Task<List<FileItem>> QueryFilesAsync(FileSearchQuery query);
    Task<long> GetTotalFilesCountAsync(string? workspaceId = null);
    Task<Dictionary<FileCategory, long>> GetCategoryCountsAsync(string? workspaceId = null);
    Task<Dictionary<string, long>> GetExtensionCountsAsync(string? workspaceId = null);
    Task ToggleFavoriteAsync(string fileId, bool isFavorite);
    Task UpdateTagsAsync(string fileId, string tags);
}
