using System;
using FileNexus.Shared.Enums;

namespace FileNexus.Shared.Models;

/// <summary>
/// Represents indexed metadata for a single physical file.
/// Files are never moved; only metadata is indexed and queried.
/// </summary>
public sealed class FileItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string WorkspaceId { get; set; } = string.Empty;
    public string FolderId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public FileCategory Category { get; set; } = FileCategory.Other;
    public string AbsolutePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? FileHash { get; set; }
    public bool IsFavorite { get; set; }
    public int ThumbnailStatus { get; set; }
    public string Tags { get; set; } = string.Empty; // Comma-separated or JSON list of tags

    /// <summary>
    /// Helper property returning formatted human-readable size (e.g., 4.2 MB)
    /// </summary>
    public string FormattedSize
    {
        get
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = Size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
