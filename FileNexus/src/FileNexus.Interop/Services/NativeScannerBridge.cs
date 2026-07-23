using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FileNexus.Interop.Native;
using FileNexus.Shared.Enums;
using FileNexus.Shared.Models;

namespace FileNexus.Interop.Services;

public sealed class NativeScannerBridge : INativeScannerBridge
{
    public bool IsNativeEngineAvailable()
    {
        try
        {
            // Test if native library can be located
            return NativeLibrary.TryLoad("filenexus_engine", typeof(NativeScannerBridge).Assembly, DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.SafeDirectories, out _);
        }
        catch
        {
            return false;
        }
    }

    public async Task<long> ScanDirectoryAsync(
        string workspaceId,
        string folderId,
        string directoryPath,
        Func<FileItem, Task> onFileDiscovered,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directoryPath))
        {
            return 0;
        }

        if (IsNativeEngineAvailable())
        {
            try
            {
                return await Task.Run(() =>
                {
                    long count = 0;
                    NativeScanCallbackDelegate callback = (namePtr, pathPtr, extPtr, size, createdSec, modifiedSec, userData) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return 0;
                        }

                        string name = Marshal.PtrToStringUTF8(namePtr) ?? string.Empty;
                        string path = Marshal.PtrToStringUTF8(pathPtr) ?? string.Empty;
                        string ext = Marshal.PtrToStringUTF8(extPtr) ?? string.Empty;

                        var fileItem = new FileItem
                        {
                            WorkspaceId = workspaceId,
                            FolderId = folderId,
                            Name = name,
                            Extension = ext.ToLowerInvariant().TrimStart('.'),
                            AbsolutePath = path,
                            Size = (long)size,
                            CreatedAt = DateTimeOffset.FromUnixTimeSeconds((long)createdSec).DateTime,
                            ModifiedAt = DateTimeOffset.FromUnixTimeSeconds((long)modifiedSec).DateTime,
                            Category = FileCategory.Other // Categorized by Core service
                        };

                        onFileDiscovered(fileItem).ConfigureAwait(false).GetAwaiter().GetResult();
                        Interlocked.Increment(ref count);
                        return 1;
                    };

                    NativeMethods.ScanDirectory(directoryPath, callback, IntPtr.Zero);
                    return count;
                }, cancellationToken);
            }
            catch
            {
                // Fallback to managed scan if native call fails unexpectedly
            }
        }

        // Managed Fallback Scanner
        return await Task.Run(async () =>
        {
            long count = 0;
            var dirInfo = new DirectoryInfo(directoryPath);
            var enumerationOptions = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System
            };

            foreach (var fileInfo in dirInfo.EnumerateFiles("*", enumerationOptions))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var item = new FileItem
                {
                    WorkspaceId = workspaceId,
                    FolderId = folderId,
                    Name = fileInfo.Name,
                    Extension = fileInfo.Extension.ToLowerInvariant().TrimStart('.'),
                    AbsolutePath = fileInfo.FullName,
                    Size = fileInfo.Length,
                    CreatedAt = fileInfo.CreationTimeUtc,
                    ModifiedAt = fileInfo.LastWriteTimeUtc,
                    Category = FileCategory.Other
                };

                await onFileDiscovered(item);
                count++;
            }

            return count;
        }, cancellationToken);
    }
}
