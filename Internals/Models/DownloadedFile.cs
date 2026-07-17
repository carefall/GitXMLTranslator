namespace RestXMLTranslator.Internals.Models
{
    public class DownloadedFile
    {
        public string Path { get; set; } = string.Empty;

        public List<HalfStringEntry> HalfEntries { get; set; } = [];

        public List<string> Ids { get; set; } = [];

        public bool Finished { get; set; }
    }
}
