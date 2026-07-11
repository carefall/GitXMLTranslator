using RestXMLTranslator.Internals.Models;
using RestXMLTranslator.Internals.Program;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
namespace RestXMLTranslator.Internals.Services
{
    public class LocalFileService
    {
        public void DeleteRedundantFiles(Dictionary<string, int> files)
        {
            string path = Path.Combine(App.Current.Settings.GameDataPath, "gamedata", "configs");
            List<string> localFiles = GetLocalFiles(path);
            if (localFiles.Count == 0) return;
            if (files.Count == 0) return;
            foreach (string item in localFiles)
            {
                if (files.ContainsKey(item)) continue;
                try
                {
                    File.Delete(Path.Combine(path, item));
                }
                catch (Exception ex)
                {
                    Logger.Log("LocalFileService", $"Unhandle exception on file {item} deletion: {ex}");
                    continue;
                }
            }
        }

        public void DeleteChanges(Dictionary<string, int> files)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Changes");
            List<string> localFiles = GetLocalFiles(path);
            if (localFiles.Count == 0) return;
            if (files.Count == 0) return;
            if (!Directory.Exists(path)) return;
            foreach (string item in localFiles)
            {
                if (files.ContainsKey(item.Replace(".json", ".xml"))) continue;
                try
                {
                    File.Delete(Path.Combine(path, item));
                }
                catch (Exception ex)
                {
                    Logger.Log("LocalFileService", $"Unhandle exception on file {item} deletion: {ex}");
                    continue;
                }
            }
        }

        public string? LoadFileText(string file)
        {
            try
            {
                return File.ReadAllText(file, Encoding.GetEncoding(1251));
            }
            catch (Exception ex)
            {
                MessageBox.Show(Locale.Get("parse_file_fail"), Locale.Get("file_load_error"));
                Logger.Log("Translator-FileRead", $"Unhandled exception: {ex}");
                return null;
            }
        }

        private XElement GetOrCreateString(XElement root, Dictionary<string, XElement> index, string id)
        {
            if (index.TryGetValue(id, out var element))
                return element;
            element = new XElement("string", new XAttribute("id", id));
            root.Add(element);
            index[id] = element;
            return element;
        }

        private XElement GetOrCreateLanguage(XElement stringElement, string tag)
        {
            XElement? element = stringElement.Element(tag);
            if (element != null) return element;
            element = new XElement(tag);
            stringElement.Add(element);
            return element;
        }

        public void ApplyHalfEntries(string path, IEnumerable<HalfStringEntry> entries)
        {
            XDocument doc = new(new XElement("string_table"));
            if (File.Exists(path)) doc = XDocument.Load(path);
            var index = doc.Root!.Elements("string").ToDictionary(x => (string)x.Attribute("id")!);
            foreach (var entry in entries)
            {
                XElement stringElement = GetOrCreateString(doc.Root!, index, entry.Id!);
                string tag = entry.Russian ? "rus" : "eng";
                XElement lang = GetOrCreateLanguage(stringElement, tag);
                lang.Value = entry.Text!;
            }
            using var writer = XmlWriter.Create(path, App.Current.XmlSettings);
            doc.Save(writer);
        }

        public async Task<SyncResult> ApplyUpdates(List<DownloadedFile> files)
        {
            try
            {
                foreach (var file in files)
                {
                    string path = Path.Combine(App.Current.Settings.GameDataPath, "gamedata", "configs", file.Path);
                    string? dir = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(dir!);
                    ApplyHalfEntries(path, file.HalfEntries);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("LocalFileService", $"Unhandled exception during changes applying: {ex}");
                return SyncResult.Other;
            }
            return SyncResult.Success;
        }

        public List<string> GetLocalFiles(string folderPath)
        {
            try
            {
                return [..Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Select(f =>
                {
                    var relative = Path.GetRelativePath(folderPath, f);
                    return relative.Replace("\\", "/");
                })];
            }
            catch (Exception ex)
            {
                Logger.Log("LocalFileService", $"Unhandled exception during local files collection: {ex}");
                return [];
            }
        }

        public async Task<ObservableCollection<FileTab>> ReadLocalFiles()
        {
            string path = Path.Combine(App.Current.Settings.GameDataPath, "gamedata", "configs");
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            ObservableCollection<FileTab> tabs = [];
            foreach (var file in files)
            {
                var relativePath = file.Replace(path, "")[1..].Replace("\\", "/");
                var tab = new FileTab(file, relativePath);
                await Task.Run(() =>
                {
                    tab.Read();
                    ApplyChanges(tab);
                });
                if (tab.Entries.Count == 0) continue;
                tabs.Add(tab);
            }
            return tabs;
        }

        private void ApplyChanges(FileTab file)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Changes", file.RelativePath.Replace(".xml", ".json"));
            if (!File.Exists(filePath)) return;
            string json = File.ReadAllText(filePath, Encoding.GetEncoding(1251));
            List<Change>? changes = JsonSerializer.Deserialize<List<Change>>(json, App.Current.JsonOptions);
            changes ??= [];
            foreach (Change change in changes)
            {
                var seq = file.Entries.Where(e => e.Id == change.Id);
                if (!seq.Any()) continue;
                StringEntry bro = seq.First();
                bro.NewEng = change.Eng;
                bro.NewRu = change.Ru;
                if (!change.IsApproved) bro.IsApproved = false;
                if (bro.HasChanges && change.IsApproved) bro.IsApproved = true;
            }
        }

        public void StoreChanges(FileTab file, bool allowApprove)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Changes", file.RelativePath.Replace(".xml", ".json"));
            string directory = Path.GetDirectoryName(filePath)!;
            Directory.CreateDirectory(directory);
            List<Change>? changes = [];
            foreach (StringEntry entry in file.Entries)
            {
                if (!entry.HasChanges) continue;
                changes.Add(new Change(entry.Id, entry.NewRu, entry.NewEng, allowApprove ? entry.IsApproved : false));
            }
            if (changes.Count == 0 && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                File.WriteAllText(filePath, JsonSerializer.Serialize(changes, App.Current.JsonOptions), Encoding.GetEncoding(1251));
            }
        }

        public ObservableCollection<StringEntry> Read(string filePath)
        {
            string xml = File.ReadAllText(filePath, Encoding.GetEncoding(1251));
            return XMLHelper.LoadStrings(xml);
        }

        public async Task ApplyApprovedChanges(FileTab tab)
        {
            XDocument doc = new(new XElement("string_entry"));
            if (File.Exists(tab.FilePath)) doc = XDocument.Load(tab.FilePath);
            var index = doc.Root!.Elements("string").ToDictionary(x => (string)x.Attribute("id")!);
            foreach (var entry in tab.Entries)
            {
                XElement stringElement = GetOrCreateString(doc.Root!, index, entry.Id!);
                XElement rus = GetOrCreateLanguage(stringElement, "rus");
                rus.Value = entry.NewRu;
                XElement eng = GetOrCreateLanguage(stringElement, "eng");
                eng.Value = entry.NewEng;
                entry.IsApproved = false;
            }
            using var writer = XmlWriter.Create(tab.FilePath, App.Current.XmlSettings);
            doc.Save(writer);
        }
    }
}
