using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Xml.Linq;

namespace RestXMLTranslator.Internals
{
    internal class RestClient
    {

        public class StringEntry
        {
            public int Uid { get; set; }
            public string? Id { get; set; }
            public string? File { get; set; }
            public string? Text { get; set; }
            public bool Russian { get; set; }
        }

        public static async Task<string> GetDataAsync(string endpoint)
        {
            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return json;
        }

        public static async Task<int> Check(string gameDataPath, int version)
        {
            try
            {
                string json = await GetDataAsync("https://nukerfall.pythonanywhere.com/translator/files");
                Dictionary<string, int> files = JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? [];
                DeleteRedundantFiles(files, gameDataPath);
                UpdateLocalFiles(files, gameDataPath, version);
                return 0;
            } catch (Exception ex)
            {
                Logger.Log("RestClient-Sync", $"Unhandled exception: {ex}");
                MessageBox.Show("Синхронизации", "Произошла неизвестная ошибка при синхронизации. Обратитесь к разработчику. К обращению приложите файл log.txt");
                return -1;
            }
        }


        public async static void UpdateLocalFiles(Dictionary<string, int> files, string gameDataPath, int version)
        {
            foreach (var file in files)
            {
                if (file.Value < version) continue;
                string update = await GetDataAsync($"https://nukerfall.pythonanywhere.com/translator/download?version={version}&filepath={file.Key}");
                List<StringEntry>? entries = JsonSerializer.Deserialize<List<StringEntry>>(update);
                if (entries == null) continue;
                string path = gameDataPath + "/gamedata/configs/" + file.Key;
                string? dir = Path.GetDirectoryName(path);
                if (dir == null)
                {
                    Logger.Log("RestClient-Drive", $"Unable to create folder {dir}");
                    MessageBox.Show($"Не удалось создать путь {dir}. Обратитесь к разработчику. К обращению приложите файл log.txt", "Синхронизация");
                    Application.Current.Shutdown();
                    return;
                }
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                XDocument doc = new(new XElement("string_table"));
                if (File.Exists(path)) doc = XDocument.Load(path);
                var index = doc.Root!.Elements("string").ToDictionary(x => (string)x.Attribute("id")!);
                foreach (var entry in entries)
                {
                    if (!index.TryGetValue(entry.Id!, out var stringElement))
                    {
                        stringElement = new XElement("string", new XAttribute("id", entry.Id!));
                        doc.Root.Add(stringElement);
                        index[entry.Id!] = stringElement;
                    }
                    var langTag = entry.Russian ? "rus" : "eng";
                    var langElement = stringElement.Element(langTag);
                    if (langElement == null)
                    {
                        langElement = new XElement(langTag);
                        stringElement.Add(langElement);
                    }
                    langElement.Value = entry.Text!;
                }
                doc.Save(path);
            }
        }

        public static void DeleteRedundantFiles(Dictionary<string, int> files, string gameDataPath)
        {
            List<string> localFiles = GetLocalFiles(gameDataPath + "/gamedata/configs");
            if (localFiles.Count == 0) return;
            if (files.Count == 0)
            {
                Directory.Delete(gameDataPath + "/gamedata/configs", true);
                return;
            }
            foreach (string item in localFiles)
            {
                if (files.ContainsKey(item)) continue;
                File.Delete(gameDataPath + "/gamedata/configs/" + item);
            }
        }

        public static List<string> GetLocalFiles(string folderPath)
        {
            try
            {
                return [.. Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Select(f => {
                    var relative = Path.GetRelativePath(folderPath, f);
                    return relative.Replace("\\", "/");
                })];
            }
            catch (Exception ex)
            {
                Logger.Log("RestClient-Drive", $"Unhandled exception: {ex}");
                return [];
            }
        }


    }
}
