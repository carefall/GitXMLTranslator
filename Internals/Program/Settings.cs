using Microsoft.Win32;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;

namespace RestXMLTranslator.Internals.Program
{
    public class Settings
    {
        public string GameDataPath { get; private set; } = "";
        public string Name { get; private set; } = "";
        public int Version { get; private set; }

        public bool LightTheme { get; private set; } = true;

        public string Language { get; private set; } = "";

        private readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        private readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public Settings()
        {
            try
            {
                Logger.Log("Settings", "Initializing settings...");
                if (!File.Exists(SettingsPath))
                {
                    Logger.Log("Settings", "Settings file not found. Creating...");
                    Save();
                }
                Load();
            }
            catch (Exception ex)
            {
                Logger.Log("Settings", $"Unhandled exception: {ex}");
                MessageBox.Show(Locale.Get("gamedata_exception"), Locale.Get("settings"), MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception();
            }
        }

        private void Load()
        {
            string json = File.ReadAllText(SettingsPath);
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                GameDataPath = root.TryGetProperty("gamedata-path", out var path) ? path.GetString() ?? "" : "";
                Name = root.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "";
                Version = root.TryGetProperty("version", out var version) ? version.GetInt32() : 0;
                LightTheme = !root.TryGetProperty("light-theme", out var lightTheme) || lightTheme.GetBoolean();
                Language = root.TryGetProperty("language", out var language) ? language.GetString() ?? "" : "";
            }
            catch (JsonException ex)
            {
                Logger.Log("Settings", $"Invalid settings JSON: {ex}");
                try
                {
                    File.Delete(SettingsPath);
                }
                catch (Exception deleteEx)
                {
                    Logger.Log("Settings", $"Unable to delete invalid settings: {deleteEx}");
                }
                GameDataPath = "";
                Name = "";
                Version = 0;
                LightTheme = true;
                Language = "";
                Save();
            }
        }

        public void UpdateVersion(int version)
        {
            Version = version;
            Save();
            Logger.Log("Settings", $"Updated version to {version} after installing update...");
        }


        public void UpdateName(string name)
        {
            Name = name;
            Save();
            Logger.Log("Settings", $"User selected name: {name}.");
        }

        public bool SelectGameDataFolder()
        {
            Logger.Log("Settings", "GameData path not found...");
            try
            {
                var result = MessageBox.Show(Locale.Get("select_gamedata_dialog"), Locale.Get("settings"), MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return false;
                }
                var dialog = new OpenFolderDialog()
                {
                    Title = Locale.Get("select_gamedata"),
                    InitialDirectory = @"C:\"
                };
                if (dialog.ShowDialog() != true)
                {
                    return false;
                }
                GameDataPath = Path.GetFullPath(dialog.FolderName);
                Directory.CreateDirectory(Path.Combine(GameDataPath, "gamedata", "configs"));
                Save();
                Logger.Log("Settings", $"GameData path selected: {GameDataPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Settings", $"Unhandled exception on gamedata folder creation: {ex}");
                MessageBox.Show(Locale.Get("gamedata_creation_failed"), Locale.Get("settings"));
                return false;
            }
        }

        public bool SelectLanguage()
        {
            Logger.Log("Settings", "Language not selected...");
            var dialog = new LanguageWindow();
            if (dialog.ShowDialog() != true) return false;
            Language = dialog.IsEnglish ? "eng" : "rus";
            Save();
            Logger.Log("Settings", $"Language selected: {Language}");
            return true;
        }

        private void Save()
        {
            var config = new Dictionary<string, object>
            {
                ["gamedata-path"] = GameDataPath,
                ["name"] = Name,
                ["version"] = Version,
                ["light-theme"] = LightTheme,
                ["language"] = Language
            };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(SettingsPath, json);
        }

        public void SwitchTheme()
        {
            LightTheme = !LightTheme;
            Save();
        }
    }
}