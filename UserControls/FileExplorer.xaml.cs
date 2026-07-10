using RestXMLTranslator.Internals.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace RestXMLTranslator.UserControls
{
    public partial class FileExplorer : UserControl
    {

        private CancellationTokenSource? _searchCancellation;
        private string _searchText = "";
        public ObservableCollection<FileTab> Files { get; } = [];

        public ICollectionView? FilesView { get; private set; }

        public FileExplorer()
        {
            InitializeComponent();
            FilesView = CollectionViewSource.GetDefaultView(Files);
            FilesView.Filter = FilterFile;
            DataContext = this;
        }

        private bool FilterFile(object obj)
        {
            if (obj is not FileTab file) return false;
            if (string.IsNullOrWhiteSpace(_searchText)) return true;
            return file.RelativePath.Contains(_searchText, StringComparison.OrdinalIgnoreCase);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchBox.Text;
            RefreshSearch();
        }

        private async void RefreshSearch()
        {
            _searchCancellation?.Cancel();
            _searchCancellation = new CancellationTokenSource();
            try
            {
                await Task.Delay(250, _searchCancellation.Token);
                FilesView?.Refresh();
            }
            catch (TaskCanceledException) { }
        }

        private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilesList.SelectedItem is not FileTab tab) return;
            App.Current.MWindow.TranslationGrid.LoadFile(tab);
        }

        public async Task<bool> SaveAll(bool allowApprove)
        {
            if (!Files.Where(f => f.HasChanges).Any()) return false;
            App.Current.MWindow.WindowBlocker.Visibility = Visibility.Visible;
            await Task.Run(() =>
            {
                foreach (var file in Files)
                {
                    if (!file.HasChanges) continue;
                    App.Current.LocalFiles.StoreChanges(file, allowApprove);
                }
            });
            App.Current.MWindow.WindowBlocker.Visibility = Visibility.Hidden;
            return true;
        }

        public bool SaveFile()
        {
            if (FilesList.SelectedItem is not FileTab tab) return false;
            if (!tab.HasChanges) return false;
            App.Current.LocalFiles.StoreChanges(tab, true);
            return true;
        }
    }
}
