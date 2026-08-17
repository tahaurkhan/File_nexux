using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileNexus.Database.Repositories;
using FileNexus.Interop.Services;
using FileNexus.Shared.Models;

namespace FileNexus.Core.Services;

public sealed class ScannerService : IScannerService
{
    private readonly INativeScannerBridge _scannerBridge;
    private readonly IFileRecordRepository _fileRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICategoryClassifier _categoryClassifier;

    private const int BatchSize = 500;

    public ScannerService(
        INativeScannerBridge scannerBridge,
        IFileRecordRepository fileRepository,
        IWorkspaceRepository workspaceRepository,
        ICategoryClassifier categoryClassifier)
    {
        _scannerBridge = scannerBridge;
        _fileRepository = fileRepository;
        _workspaceRepository = workspaceRepository;
        _categoryClassifier = categoryClassifier;
    }

    public async Task<long> ScanFolderAsync(
        Workspace workspace,
        IndexedFolder folder,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var buffer = new List<FileItem>(BatchSize);
        long totalScanned = 0;

        // Clear existing file records for this folder before re-indexing
        await _fileRepository.DeleteByFolderIdAsync(folder.Id);

        totalScanned = await _scannerBridge.ScanDirectoryAsync(
            workspace.Id,
            folder.Id,
            folder.Path,
            async (item) =>
            {
                item.Category = _categoryClassifier.Classify(item.Extension);
                buffer.Add(item);

                if (buffer.Count >= BatchSize)
                {
                    var chunk = buffer.ToArray();
                    buffer.Clear();
                    await _fileRepository.BulkUpsertBatchAsync(chunk);
                    progress?.Report((int)totalScanned);
                }
            },
            cancellationToken
        );

        if (buffer.Count > 0)
        {
            await _fileRepository.BulkUpsertBatchAsync(buffer);
            buffer.Clear();
        }

        await _workspaceRepository.UpdateFolderLastScannedAsync(folder.Id, totalScanned);
        progress?.Report((int)totalScanned);

        return totalScanned;
    }
}
