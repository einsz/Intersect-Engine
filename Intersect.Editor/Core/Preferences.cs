using System.Text.Json;
using Intersect.Configuration;

namespace Intersect.Editor.Core;

public static partial class Preferences
{
    private static bool? _enableCursorSprites;

    private static readonly string PreferencesFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "IntersectEditor",
        "preferences.json"
    );

    private static Dictionary<string, string>? _preferences;

    private static Dictionary<string, string> LoadPreferences()
    {
        if (_preferences != null)
        {
            return _preferences;
        }

        try
        {
            var dir = Path.GetDirectoryName(PreferencesFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(PreferencesFilePath))
            {
                var json = File.ReadAllText(PreferencesFilePath);
                _preferences = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
            else
            {
                _preferences = new Dictionary<string, string>();
            }
        }
        catch
        {
            _preferences = new Dictionary<string, string>();
        }

        return _preferences;
    }

    private static void SaveAllPreferences()
    {
        try
        {
            var dir = Path.GetDirectoryName(PreferencesFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(_preferences, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PreferencesFilePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    public static bool EnableCursorSprites
    {
        get => _enableCursorSprites ??= LoadPreferenceBool(nameof(EnableCursorSprites)) ?? false;
        set
        {
            if (_enableCursorSprites == value)
            {
                return;
            }

            _enableCursorSprites = value;
            SavePreference(nameof(EnableCursorSprites), _enableCursorSprites.ToString() ?? string.Empty);
        }
    }

    public static void SavePreference(string key, string value)
    {
        var prefs = LoadPreferences();
        var storeKey = $"{ClientConfiguration.Instance.Host}:{ClientConfiguration.Instance.Port}:{key}";
        prefs[storeKey] = value;
        SaveAllPreferences();
    }

    private static bool? LoadPreferenceBool(string key)
    {
        var rawPreference = LoadPreference(key);
        if (string.IsNullOrWhiteSpace(rawPreference))
        {
            return null;
        }

        return Convert.ToBoolean(rawPreference);
    }

    public static string LoadPreference(string key)
    {
        try
        {
            var prefs = LoadPreferences();
            var storeKey = $"{ClientConfiguration.Instance.Host}:{ClientConfiguration.Instance.Port}:{key}";
            return prefs.TryGetValue(storeKey, out var value) ? value : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
