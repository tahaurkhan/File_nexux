using System;

namespace FileNexus.Shared.Models;

/// <summary>
/// Represents a physical directory included in a FileNexus workspace for scanning and indexing.
/// </summary>
public sealed class IndexedFolder
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string WorkspaceId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime? LastScannedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public long FileCount { get; set; }
}
