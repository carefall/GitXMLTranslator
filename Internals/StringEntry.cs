namespace GitXMLTranslator.Internals
{
    public class StringEntry(string id, string oldRu, string newRu, string oldEn, string newEn, string comment)
    {
        public string id = id, oldRu = oldRu, newRu = newRu, oldEn = oldEn, newEn = newEn, comment = comment;

        public static string DecodeMultiline(string text)
        {
            return text.Replace("\\n", Environment.NewLine);
        }

        public static string EncodeMultiline(string text)
        {
            return text.Replace("\r\n", "\\n").Replace("\n", "\\n");
        }
    }
}
