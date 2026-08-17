using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace FileNexus.UI.Services;

public static class ThumbnailService
{
    private static readonly ConcurrentDictionary<string, Bitmap?> ThumbnailCache = new();

    public static bool IsSupportedImageExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return false;
        string ext = extension.TrimStart('.').ToLowerInvariant();
        return ext is "jpg" or "jpeg" or "png" or "gif" or "bmp" or "webp" or "ico" or "svg";
    }

    public static async Task<Bitmap?> LoadThumbnailAsync(string path, int maxDimension = 300)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        if (ThumbnailCache.TryGetValue(path, out var cached))
            return cached;

        return await Task.Run(() =>
        {
            try
            {
                using var stream = File.OpenRead(path);
                var bitmap = Bitmap.DecodeToWidth(stream, maxDimension);
                ThumbnailCache[path] = bitmap;
                return bitmap;
            }
            catch
            {
                ThumbnailCache[path] = null;
                return null;
            }
        });
    }

    public static Bitmap? GetCachedThumbnail(string path)
    {
        return ThumbnailCache.TryGetValue(path, out var cached) ? cached : null;
    }
}
