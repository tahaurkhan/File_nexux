using System.Collections.Generic;
using System.Threading.Tasks;
using FileNexus.Database.Repositories;
using FileNexus.Shared.Enums;
using FileNexus.Shared.Models;

namespace FileNexus.Core.Services;

public sealed class FileService : IFileService
{
    private readonly IFileRecordRepository _fileRepository;

    public FileService(IFileRecordRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public Task<List<FileItem>> QueryFilesAsync(FileSearchQuery query)
    {
        return _fileRepository.SearchAsync(query);
    }

    public Task<long> GetTotalFilesCountAsync(string? workspaceId = null)
    {
        return _fileRepository.GetTotalCountAsync(workspaceId);
    }

    public Task<Dictionary<FileCategory, long>> GetCategoryCountsAsync(string? workspaceId = null)
    {
        return _fileRepository.GetCategoryCountsAsync(workspaceId);
    }

    public Task<Dictionary<string, long>> GetExtensionCountsAsync(string? workspaceId = null)
    {
        return _fileRepository.GetExtensionCountsAsync(workspaceId);
    }

    public Task ToggleFavoriteAsync(string fileId, bool isFavorite)
    {
        return _fileRepository.ToggleFavoriteAsync(fileId, isFavorite);
    }

    public Task UpdateTagsAsync(string fileId, string tags)
    {
        return _fileRepository.UpdateTagsAsync(fileId, tags);
    }
}
