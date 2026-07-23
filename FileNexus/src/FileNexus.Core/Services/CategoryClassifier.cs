using System;
using System.Collections.Generic;
using FileNexus.Shared.Enums;

namespace FileNexus.Core.Services;

public sealed class CategoryClassifier : ICategoryClassifier
{
    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "doc", "docx", "txt", "rtf", "odt", "xls", "xlsx", "csv", "ppt", "pptx", "md", "tex", "wpd"
    };

    private static readonly HashSet<string> BookExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "pdf", "epub", "mobi", "azw", "azw3", "djvu", "cbr", "cbz", "fb2"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "jpg", "jpeg", "png", "gif", "bmp", "webp", "svg", "ico", "tiff", "psd", "ai", "heic", "raw", "cr2", "nef"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "mp4", "mkv", "avi", "mov", "wmv", "flv", "webm", "m4v", "mpg", "mpeg", "3gp"
    };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "mp3", "wav", "flac", "aac", "ogg", "wma", "m4a", "alac", "aiff", "mid", "midi"
    };

    private static readonly HashSet<string> CodeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "cs", "java", "py", "rs", "cpp", "c", "h", "hpp", "js", "ts", "jsx", "tsx", "html", "css", "scss", "json", "xml", "yaml", "yml", "sh", "bash", "ps1", "sql", "go", "php", "rb", "swift", "kt", "scala", "dart"
    };

    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "zip", "tar", "gz", "7z", "rar", "bz2", "xz", "iso", "tgz"
    };

    private static readonly HashSet<string> ExecutableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "exe", "dll", "so", "dylib", "bin", "sh", "appImage", "msi", "bat", "cmd", "apk", "deb", "rpm"
    };

    public FileCategory Classify(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return FileCategory.Other;
        }

        string cleanExt = extension.TrimStart('.').ToLowerInvariant();

        if (BookExtensions.Contains(cleanExt)) return FileCategory.Books;
        if (DocumentExtensions.Contains(cleanExt)) return FileCategory.Documents;
        if (ImageExtensions.Contains(cleanExt)) return FileCategory.Images;
        if (VideoExtensions.Contains(cleanExt)) return FileCategory.Videos;
        if (AudioExtensions.Contains(cleanExt)) return FileCategory.Audio;
        if (CodeExtensions.Contains(cleanExt)) return FileCategory.Code;
        if (ArchiveExtensions.Contains(cleanExt)) return FileCategory.Archives;
        if (ExecutableExtensions.Contains(cleanExt)) return FileCategory.Executables;

        return FileCategory.Other;
    }
}
