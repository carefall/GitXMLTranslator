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

        public Dictionary<string, bool> Statuses { get; private set; } = [];

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
                Logger.Log("Initializing settings...", "Settings");
                if (!File.Exists(SettingsPath))
                {
                    Logger.Log("Settings file not found. Creating...", "Settings");
                    Save();
                }
                Load();
            }
            catch (Exception ex)
            {
                Logger.Log($"Unhandled exception: {ex}", "Settings");
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
                Statuses = root.TryGetProperty("statuses", out var statuses) ? statuses.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetBoolean()) : [];
            }
            catch (JsonException ex)
            {
                Logger.Log($"Invalid settings JSON: {ex}", "Settings");
                try
                {
                    File.Delete(SettingsPath);
                }
                catch (Exception deleteEx)
                {
                    Logger.Log($"Unable to delete invalid settings: {deleteEx}", "Settings");
                }
                GameDataPath = "";
                Name = "";
                Version = 0;
                LightTheme = true;
                Language = "";
                Statuses = [];
                Save();
            }
        }

        public void UpdateVersion(int version)
        {
            Version = version;
            Save();
            Logger.Log($"Updated version to {version} after installing update...", "Settings");
        }


        public void UpdateName(string name)
        {
            Name = name;
            Save();
            Logger.Log($"User selected name: {name}.", "Settings");
        }

        public bool SelectGameDataFolder()
        {
            Logger.Log("GameData path not found...", "Settings");
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
                Logger.Log($"GameData path selected: {GameDataPath}", "Settings");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Unhandled exception on gamedata folder creation: {ex}", "Settings");
                MessageBox.Show(Locale.Get("gamedata_creation_failed"), Locale.Get("settings"));
                return false;
            }
        }

        public bool SelectLanguage()
        {
            Logger.Log("Language not selected...", "Settings");
            var dialog = new LanguageWindow();
            if (dialog.ShowDialog() != true) return false;
            Language = dialog.IsEnglish ? "eng" : "rus";
            Save();
            Logger.Log($"Language selected: {Language}", "Settings");
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
                ["language"] = Language,
                ["statuses"] = Statuses
            };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(SettingsPath, json);
        }

        public void SwitchTheme()
        {
            LightTheme = !LightTheme;
            Save();
        }

        public bool GetFileStatus(string file)
        {
            return Statuses.GetValueOrDefault(file, false);
        }

        public void TryDeleteStatus(string file)
        {
            Statuses.Remove(file);
            Save();
        }

        public void SetOrAddFileStatus(string file, bool status)
        {
            Statuses[file] = status;
            Save();
        }
    }
}