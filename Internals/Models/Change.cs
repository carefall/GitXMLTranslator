namespace RestXMLTranslator.Internals.Models
{
    public class Change(string Id, string Ru, string Eng, bool IsApproved, string Comment)
    {
        public string Id { get; set; } = Id;

        public string Ru { get; set; } = Ru;

        public string Eng { get; set; } = Eng;

        public bool IsApproved { get; set; } = IsApproved;

        public string Comment { get; set; } = Comment;
    }
}
