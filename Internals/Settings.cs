using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace GitXMLTranslator.Internals
{
    internal class Settings
    {

        public string gamedataPath = "";
        public string name = "";
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        public Settings()
        {
            try
            {
                string folderPath = AppDomain.CurrentDomain.BaseDirectory + "/Assets";
                string settingsPath = AppDomain.CurrentDomain.BaseDirectory + "/Assets/settings.json";
                if (!File.Exists(settingsPath))
                {
                    Directory.CreateDirectory(folderPath);
                    //File.Create(settingsPath).Close();
                    var config = new Dictionary<string, string>
                    {
                        ["gamedata-path"] = "",
                        ["name"] = ""
                    };
                    string data = JsonSerializer.Serialize(config, options);
                    File.WriteAllText(settingsPath, data);
                }
                string json = File.ReadAllText(settingsPath);
                using JsonDocument doc = JsonDocument.Parse(json);
                string? value = doc.RootElement.GetProperty("gamedata-path").GetString();
                name = doc.RootElement.GetProperty("name").GetString() ?? "";
                if (value == null || value.Trim().Length == 0)
                {
                    var dialog = new OpenFolderDialog
                    {
                        Title = "Выберите папку с gamedata",
                        InitialDirectory = @"C:\"
                    };
                    while (dialog.ShowDialog() != true)
                    {
                        var result = MessageBox.Show("Выберите папку, куда будут размещены файлы(папка gamedata и её содержимое)", "Настройка", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.No) Application.Current.Shutdown();
                    }
                    string path = dialog.FolderName;
                    gamedataPath = path;
                    var config = new Dictionary<string, string>
                    {
                        ["gamedata-path"] = path,
                        ["name"] = ""
                    };
                    string data = JsonSerializer.Serialize(config, options);
                    File.WriteAllText(settingsPath, data);
                    Directory.CreateDirectory(path + "/gamedata/configs");
                    return;
                }
                gamedataPath = value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Application.Current.Shutdown();
            }
        }

        public void UpdateName(string name)
        {
            this.name = name;
            try
            {
                string settingsPath = AppDomain.CurrentDomain.BaseDirectory + "/Assets/settings.json";
                string json = File.ReadAllText(settingsPath);
                using JsonDocument doc = JsonDocument.Parse(json);
                var config = new Dictionary<string, string>
                {
                    ["gamedata-path"] = doc.RootElement.GetProperty("gamedata-path").GetString() ?? "",
                    ["name"] = name
                };
                string data = JsonSerializer.Serialize(config, options);
                File.WriteAllText(settingsPath, data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Application.Current.Shutdown();
            }

        }
    }
}
