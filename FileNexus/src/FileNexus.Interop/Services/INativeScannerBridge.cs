using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileNexus.Shared.Models;

namespace FileNexus.Interop.Services;

public interface INativeScannerBridge
{
    bool IsNativeEngineAvailable();

    Task<long> ScanDirectoryAsync(
        string workspaceId,
        string folderId,
        string directoryPath,
        Func<FileItem, Task> onFileDiscovered,
        CancellationToken cancellationToken = default
    );
}
