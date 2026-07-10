using RestXMLTranslator.Internals.Models;
using RestXMLTranslator.Internals.Program;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RestXMLTranslator.UserControls
{
    public partial class TranslationGrid : UserControl, INotifyPropertyChanged
    {

        private ObservableCollection<StringEntry> _entries = [];

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<StringEntry> Entries
        {
            get => _entries; set
            {
                _entries = value;
                OnPropertyChanged();
                EntriesView?.Refresh();
            }
        }

        public ICollectionView? EntriesView;

        public TranslationGrid()
        {
            InitializeComponent();
            EntriesView = CollectionViewSource.GetDefaultView(Entries);
            EntriesView.Filter = FilterEntries;
            DataContext = this;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool FilterEntries(object obj)
        {
            if (obj is not StringEntry entry) return false;
            var buttons = App.Current.MWindow.Buttons;
            if (buttons.HideApproved.IsChecked == true && entry.IsApproved) return false;
            if (buttons.HideChanged.IsChecked == true && entry.HasChanges) return false;
            if (buttons.HideUnchanged.IsChecked == true && !entry.HasChanges) return false;
            if (!string.IsNullOrWhiteSpace(buttons.SearchText))
            {
                string s = buttons.SearchText.ToLowerInvariant();
                return ContainTextAny([entry.Id, entry.NewRu, entry.NewEng, entry.Ru, entry.Eng], s);
            }
            return true;
        }

        private bool ContainTextAny(string[] strings, string s)
        {
            return strings.Where(str => str.Contains(s, StringComparison.OrdinalIgnoreCase)).Any();
        }


        internal void LoadFile(FileTab tab)
        {
            StringEntry? targetEntry = null;
            Entries.Clear();
            foreach (var entry in tab.Entries)
            {
                Entries.Add(entry);
                if (tab.SelectedEntry != "" && tab.SelectedEntry == entry.Id) targetEntry = entry;
            }
            if (targetEntry != null) {
                TGrid.SelectedItem = targetEntry;
                TGrid.ScrollIntoView(targetEntry);
            }

        }

        private void TextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            tb.CaretIndex = tb.Text.Length;
            tb.SelectionLength = 0;
        }

        private void TGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item is StringEntry entry && entry.IsApproved)
            {
                e.Cancel = true;
            }
        }

        private void EditLongText(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGridCell cell) return;
            if (cell.DataContext is not StringEntry entry) return;
            if (entry.IsApproved) return;
            if (cell.Column is not DataGridTextColumn column || column.Binding is not Binding binding) return;
            var property = typeof(StringEntry).GetProperty(binding.Path.Path);
            if (property == null) return;
            if (!property.CanWrite) return;
            var dlg = new TextEditWindow(property.GetValue(entry)?.ToString() ?? "")
            {
                Owner = Window.GetWindow(this)
            };
            if (dlg.ShowDialog() == true)
            {
                property.SetValue(entry, dlg.ResultText);
            }
            e.Handled = true;
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridCell cell && !cell.IsEditing && !cell.IsReadOnly)
            {
                cell.Focus();
                if (TGrid.BeginEdit(e))
                {
                    e.Handled = true;
                }
                TGrid.SelectedItem = cell.DataContext;
                if (App.Current.MWindow.Files.FilesList.SelectedItem is not FileTab tab) return;
                if (TGrid.SelectedItem is not StringEntry entry) return;
                tab.SelectedEntry = entry.Id;
            }
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TGrid.SelectedItems)
            {
                if (item is StringEntry entry && entry.HasChanges)
                {
                    entry.IsApproved = !entry.IsApproved;
                }
            }
        }

        public void InsertTranslations(Dictionary<string, StringEntry> translations, bool file)
        {
            if (translations.Count == 0) return;
            foreach (var item in Entries)
            {
                if (translations.TryGetValue(item.Id!, out var tr) &&
                    !string.IsNullOrWhiteSpace(tr.Eng))
                {
                    item.NewEng = tr.Eng;
                }
                if (translations.TryGetValue(item.Id, out var tr2) &&
                    !string.IsNullOrWhiteSpace(tr2.Ru))
                {
                    item.NewRu = tr2.Ru;
                }
            }
            EntriesView?.Refresh();
            MessageBox.Show(Locale.Get(file ? "loaded_from_file" : "clipboard_loaded"), Locale.Get("translation"));
        }
    }
}
