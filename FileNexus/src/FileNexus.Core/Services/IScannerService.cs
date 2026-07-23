using System;
using System.Threading;
using System.Threading.Tasks;
using FileNexus.Shared.Models;

namespace FileNexus.Core.Services;

public interface IScannerService
{
    Task<long> ScanFolderAsync(
        Workspace workspace,
        IndexedFolder folder,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default
    );
}
