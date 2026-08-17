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
    /// Helper property returning appropriate icon emoji based on extension and category.
    /// </summary>
    public string Icon
    {
        get
        {
            string ext = Extension.TrimStart('.').ToLowerInvariant();
            return ext switch
            {
                "pdf" => "📕",
                "doc" or "docx" or "rtf" or "odt" => "📘",
                "xls" or "xlsx" or "csv" or "ods" => "📗",
                "ppt" or "pptx" or "odp" => "📙",
                "txt" or "md" or "log" => "📝",
                "jpg" or "jpeg" or "png" or "gif" or "webp" or "svg" or "bmp" or "ico" => "🖼️",
                "mp4" or "mkv" or "avi" or "mov" or "webm" or "flv" or "wmv" or "m4v" => "🎥",
                "mp3" or "flac" or "wav" or "aac" or "m4a" or "ogg" or "wma" or "opus" => "🎵",
                "cs" or "rs" or "ts" or "js" or "py" or "cpp" or "c" or "h" or "html" or "css" or "json" or "xml" or "sh" or "sql" => "💻",
                "zip" or "rar" or "7z" or "tar" or "gz" or "iso" or "bz2" or "xz" => "📦",
                "exe" or "msi" or "app" or "deb" or "rpm" or "bin" or "apk" => "⚙️",
                _ => Category switch
                {
                    FileCategory.Documents or FileCategory.Books => "📄",
                    FileCategory.Images => "🖼️",
                    FileCategory.Videos => "🎥",
                    FileCategory.Audio => "🎵",
                    FileCategory.Code => "💻",
                    FileCategory.Archives => "📦",
                    FileCategory.Executables => "⚙️",
                    _ => "📁"
                }
            };
        }
    }

    /// <summary>
    /// Helper property returning vibrant color for extension badge.
    /// </summary>
    public string BadgeColor
    {
        get
        {
            string ext = Extension.TrimStart('.').ToLowerInvariant();
            return ext switch
            {
                "pdf" => "#EF4444",
                "doc" or "docx" => "#2563EB",
                "xls" or "xlsx" or "csv" => "#16A34A",
                "ppt" or "pptx" => "#D97706",
                "jpg" or "jpeg" or "png" or "webp" or "svg" => "#10B981",
                "mp4" or "mkv" or "avi" or "mov" => "#F59E0B",
                "mp3" or "flac" or "wav" => "#8B5CF6",
                "cs" or "rs" or "ts" or "js" or "py" => "#06B6D4",
                "zip" or "rar" or "7z" or "tar" => "#64748B",
                "exe" or "bin" or "sh" => "#DC2626",
                _ => "#38BDF8"
            };
        }
    }

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

    /// <summary>
    /// Helper property returning upper-case extension (e.g. JPG, PDF)
    /// </summary>
    public string UpperExtension => string.IsNullOrWhiteSpace(Extension) ? "FILE" : Extension.TrimStart('.').ToUpperInvariant();

    /// <summary>
    /// Returns true if this file is an image format suitable for thumbnail preview
    /// </summary>
    public bool IsImage
    {
        get
        {
            string ext = Extension.TrimStart('.').ToLowerInvariant();
            return ext is "jpg" or "jpeg" or "png" or "gif" or "bmp" or "webp" or "ico" or "svg";
        }
    }
}
