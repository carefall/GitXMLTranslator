using RestXMLTranslator.Internals.Models;
using RestXMLTranslator.Internals.Program;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace RestXMLTranslator.Internals.Services
{
    public class LocalFileService
    {
        public void DeleteRedundantFiles(Dictionary<string, int> files, string gameDataPath)
        {
            List<string> localFiles = GetLocalFiles(Path.Combine(gameDataPath, "gamedata", "configs"));
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

        public void DeleteRedundantFilesWithTabs(Dictionary<string, int> files, ObservableCollection<FileTab> tabs)
        {
            var filesToDelete = tabs.Where(t => !files.ContainsKey(t.RelativePath.Replace("\\", "/"))).ToList();
            foreach (var tab in filesToDelete)
            {
                tabs.Remove(tab);
                File.Delete(tab.FilePath);
                string changesPath = AppDomain.CurrentDomain.BaseDirectory + "Changes/" + tab.RelativePath.Replace("\\", "/").Replace(".xml", ".json");
                if (File.Exists(changesPath))
                {
                    File.Delete(changesPath);
                }
            }
        }


        public int ApplyUpdates(string gameDataPath, List<DownloadedFile> files)
        {
            foreach (var file in files)
            {
                string path = Path.Combine(gameDataPath, "gamedata", "configs", file.Path);
                string? dir = Path.GetDirectoryName(path);
                if (dir == null)
                {
                    Logger.Log("LocalFileService", $"Unable to create folder {dir}");
                    MessageBox.Show(Locale.Get("sync_fail", dir ?? "UNDEFINED"), Locale.Get("sync"));
                    return -1;
                }
                Directory.CreateDirectory(dir);
                ApplyHalfEntries(path, file.HalfEntries);
            }

            return 0;
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
                Logger.Log("LocalFileService", $"Unhandled exception: {ex}");
                return [];
            }
        }
    }
}
