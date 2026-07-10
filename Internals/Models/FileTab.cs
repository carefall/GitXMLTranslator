using System.Collections.ObjectModel;
using System.IO;

namespace RestXMLTranslator.Internals.Models
{
    public class FileTab
    {

        public string SelectedEntry { get; set; } = "";

        public string Name { get; set; } = "";
        public string FilePath { get; set; } = "";

        public string RelativePath { get; set; } = "";

        public string Tip { get; set; } = "";

        public ObservableCollection<StringEntry> Entries { get; set; } = [];

        public FileTab(string path, string relativePath)
        {
            FilePath = path;
            RelativePath = relativePath;
            Tip = RelativePath;
            Name = Path.GetFileName(path);
        }

        public void Read()
        {
            Entries = App.Current.LocalFiles.Read(FilePath);
        }

        public bool HasApprovedChanges => Entries.Where(e => e.IsApproved).Any();

        public bool HasChanges => Entries.Where(e => e.HasChanges).Any();
    }
}
