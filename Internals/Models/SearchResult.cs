namespace RestXMLTranslator.Internals.Models
{
    public class SearchResult
    {
        public FileTab File { get; set; } = null!;

        public string FileName => File.Name;

        public string Id { get; set; } = "";

        public string Preview { get; set; } = "";

        public StringEntry Entry { get; set; } = null!;

        public string Type { get; set; } = "";
    }
}
