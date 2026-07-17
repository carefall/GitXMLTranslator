using RestXMLTranslator.Internals.Models;
using System.Windows;
using System.Windows.Input;

namespace RestXMLTranslator
{
    public partial class SearchWindow : Window
    {
        public SearchWindow()
        {
            InitializeComponent();
        }

        private void ResultsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsGrid.SelectedItem is SearchResult result)
            {
                App.Current.MWindow.NavigateTo(result.File, result.Id);
                Close();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string search = SearchBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(search))
                return;

            List<SearchResult> results = [];

            foreach (FileTab file in App.Current.MWindow.Files.Files)
            {
                foreach (StringEntry entry in file.Entries)
                {
                    AddResult(results, file, entry.Id, entry.Id, search);
                    AddResult(results, file, entry.Id, entry.Ru, search);
                    AddResult(results, file, entry.Id, entry.NewRu, search);
                    AddResult(results, file, entry.Id, entry.Eng, search);
                    AddResult(results, file, entry.Id, entry.NewEng, search);
                    AddResult(results, file, entry.Id, entry.Comment, search);
                    AddResult(results, file, entry.Id, entry.NewComment, search);
                }
            }
            ResultsGrid.ItemsSource = results;
            if (results.Count > 0)
            {
                ResultsGrid.SelectedIndex = 0;
                ResultsGrid.ScrollIntoView(results[0]);
                ResultsGrid.Focus();
            }
        }

        private static void AddResult(List<SearchResult> results, FileTab file, string id, string? text, string search)
        {
            if (!Contains(text, search))
                return;
            results.Add(new SearchResult
            {
                File = file,
                Id = id,
                Preview = CreatePreview(text!, search)
            });
        }

        private static bool Contains(string? text, string search)
        {
            return !string.IsNullOrEmpty(text) &&
                   text.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        private static string CreatePreview(string text, string search)
        {
            text = text.Replace('\n', ' ')
                       .Replace('\r', ' ');

            int index = text.IndexOf(search, StringComparison.OrdinalIgnoreCase);

            if (index < 0)
                return text.Length > 120 ? text[..120] + "..." : text;

            const int radius = 40;

            int start = Math.Max(0, index - radius);
            int length = Math.Min(
                text.Length - start,
                search.Length + radius * 2
            );

            string preview = text.Substring(start, length);

            if (start > 0)
                preview = "..." + preview;

            if (start + length < text.Length)
                preview += "...";

            return preview;
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchButton_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }
    }
}
