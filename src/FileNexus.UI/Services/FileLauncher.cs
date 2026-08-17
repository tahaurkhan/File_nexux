using System;
using System.Diagnostics;
using System.IO;

namespace FileNexus.UI.Services;

public static class FileLauncher
{
    /// <summary>
    /// Opens the specified file using the operating system's default application.
    /// Works cross-platform (Linux xdg-open, Windows UseShellExecute, macOS open).
    /// </summary>
    public static bool OpenFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            if (OperatingSystem.IsLinux())
            {
                Process.Start(new ProcessStartInfo("xdg-open", $"\"{filePath}\"") { UseShellExecute = false });
            }
            else if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start(new ProcessStartInfo("open", $"\"{filePath}\"") { UseShellExecute = false });
            }
            else
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            return true;
        }
        catch
        {
            try
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Reveals the specified file in its parent folder using the OS native file manager.
    /// </summary>
    public static bool OpenFolder(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;

        string? dir = Directory.Exists(filePath) ? filePath : Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return false;

        try
        {
            if (OperatingSystem.IsLinux())
            {
                Process.Start(new ProcessStartInfo("xdg-open", $"\"{dir}\"") { UseShellExecute = false });
            }
            else if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{filePath}\"") { UseShellExecute = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start(new ProcessStartInfo("open", $"\"{dir}\"") { UseShellExecute = false });
            }
            else
            {
                Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true });
            }
            return true;
        }
        catch
        {
            try
            {
                Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
