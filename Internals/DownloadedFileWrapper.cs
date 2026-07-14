using RestXMLTranslator.Internals.Models;
using RestXMLTranslator.Internals.Program;

namespace RestXMLTranslator.Internals
{
    internal class DownloadedFileWrapper
    {
        public static void FillEntries(DownloadedFile file)
        {
            List<StringEntry> entries = [];
            foreach (var entry in file.HalfEntries)
            {
                Logger.Log(entry.Id.ToString());
                Logger.Log(entry.EditType.ToString());
                bool ru = entry.EditType == 0;
                bool eng = entry.EditType == 1;
                bool com = entry.EditType == -1;
                string text = entry.Text ?? "";
                var existing = entries.FirstOrDefault(e => e.Id == entry.Id);
                if (existing == null)
                {
                    entries.Add(new StringEntry()
                    {
                        Id = entry.Id!,
                        downloadedRu = ru,
                        downloadedEng = eng,
                        downloadedComment = com,
                        Ru = ru? text : "",
                        NewRu = ru ? text : "",
                        Eng = eng ? text : "",
                        NewEng = eng ? text : "",
                        Comment = com ? text : "",
                        NewComment = com ? text : "" 
                    });

                    continue;
                }
                if (ru)
                {
                    existing.downloadedRu = true;
                    existing.Ru = text;
                    existing.NewRu = text;
                }
                else if (eng)
                {
                    existing.downloadedEng = true;
                    existing.Eng = text;
                    existing.NewEng = text;
                }
                else
                {
                    existing.downloadedComment = true;
                    existing.Comment = text;
                    existing.Comment = text;
                }
            }
            file.Entries = entries;
        }

        public static void FillEntries(IEnumerable<DownloadedFile> files)
        {
            foreach (var file in files)
            {
                FillEntries(file);
            }
        }
    }
}
