using RestXMLTranslator.Internals.Program;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RestXMLTranslator.Internals.Models
{
    public class FileTab
    {
        public string Name { get; set; } = "";
        public string FilePath { get; set; } = "";

        public string RelativePath { get; set; } = "";

        public string Tip { get; set; } = "";

        public ObservableCollection<StringEntry> Entries { get; set; } = [];

        public FileTab(string path, string relativePath, bool read)
        {
            FilePath = path;
            RelativePath = relativePath;
            Tip = RelativePath;
            Name = Path.GetFileName(path);
            if (!read) return;
            string xml = File.ReadAllText(path, Encoding.GetEncoding(1251));
            Entries = XMLHelper.LoadStrings(xml);
        }

        public void WriteToDisk(List<StringEntry> entries)
        {
            FilePath = App.Current.Settings.GameDataPath + "/gamedata/configs/" + RelativePath;
            Entries = new ObservableCollection<StringEntry>(entries);
            Name = Path.GetFileName(FilePath);
            string dir = Path.GetDirectoryName(FilePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            XDocument doc = new(new XElement("string_table"));
            if (File.Exists(FilePath)) doc = XDocument.Load(FilePath);
            var index = doc.Root!.Elements("string").ToDictionary(x => (string)x.Attribute("id")!);
            foreach (var entry in entries)
            {
                if (!index.TryGetValue(entry.Id!, out var stringElement))
                {
                    stringElement = new XElement("string", new XAttribute("id", entry.Id!));
                    doc.Root.Add(stringElement);
                    index[entry.Id!] = stringElement;
                }
                var node = doc.Root!;
                var rus = node.Element("rus");
                string text1 = XMLHelper.EncodeMultiline(entry.Ru);
                if (rus == null) node.Add(new XElement("rus", text1));
                else rus.Value = text1;
                var eng = node.Element("eng");
                string text2 = XMLHelper.EncodeMultiline(entry.Eng);
                if (eng == null) node.Add(new XElement("eng", text2));
                else eng.Value = text2;
            }
            using var writer = XmlWriter.Create(FilePath, App.Current.XmlSettings);
            doc.Save(writer);
        }

        public bool HasApprovedChanges => Entries.Where(e => e.IsApproved).Any();

        public bool HasChanges => Entries.Where(e => e.HasChanges).Any();
    }
}
