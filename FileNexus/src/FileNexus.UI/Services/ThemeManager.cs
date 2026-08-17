using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Styling;

namespace FileNexus.UI.Services;

public enum ThemeOption
{
    System,
    Light,
    Dark
}

public static class ThemeManager
{
    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "filenexus", "settings.json");

    public static ThemeOption CurrentTheme { get; private set; } = ThemeOption.System;

    public static void Initialize()
    {
        CurrentTheme = LoadThemePreference();
        ApplyTheme(CurrentTheme);
    }

    public static void ApplyTheme(ThemeOption theme)
    {
        CurrentTheme = theme;
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = theme switch
            {
                ThemeOption.Light => ThemeVariant.Light,
                ThemeOption.Dark => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
        }
        SaveThemePreference(theme);
    }

    public static ThemeOption CycleNextTheme()
    {
        ThemeOption next = CurrentTheme switch
        {
            ThemeOption.System => ThemeOption.Light,
            ThemeOption.Light => ThemeOption.Dark,
            _ => ThemeOption.System
        };
        ApplyTheme(next);
        return next;
    }

    private static ThemeOption LoadThemePreference()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Theme", out var themeProp))
                {
                    string themeStr = themeProp.GetString() ?? "System";
                    if (Enum.TryParse<ThemeOption>(themeStr, true, out var result))
                    {
                        return result;
                    }
                }
            }
        }
        catch { }
        return ThemeOption.System;
    }

    private static void SaveThemePreference(ThemeOption theme)
    {
        try
        {
            string? dir = Path.GetDirectoryName(SettingsFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var options = new { Theme = theme.ToString() };
            string json = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch { }
    }
}
