using RestXMLTranslator.Internals.Models;
using RestXMLTranslator.Internals.Program;
using System.Windows;
using System.Windows.Input;

namespace RestXMLTranslator
{
    public partial class MainWindow : Window
    {
        public MainWindow(bool online)
        {
            InitializeComponent();
            Title = Locale.Get("window_title", online ? Locale.Get("connected", GetCurrentTimeHM()) : Locale.Get("not_connected"));
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow = this;
            Files.Files.Clear();
            foreach (var file in await App.Current.LocalFiles.ReadLocalFiles())
            {
                Files.Files.Add(file);
            }
            Files.FilesView?.Refresh();
            Files.FilesList.SelectedIndex = 0;
        }

        public static string GetCurrentTimeHM() => DateTime.Now.ToString("HH:mm");

        public async Task Download()
        {
            WindowBlocker.Visibility = Visibility.Visible;
            int version = await App.Current.SyncService.CompareVersions();
            if (version == -1)
            {
                WindowBlocker.Visibility = Visibility.Hidden;
                MessageBox.Show(Locale.Get("server_unreachable"), Locale.Get("sync"), MessageBoxButton.OK, MessageBoxImage.Error);
                Title = Locale.Get("window_title", Locale.Get("not_connected"));
                return;
            }
            if (version == -2)
            {
                WindowBlocker.Visibility = Visibility.Hidden;
                MessageBox.Show(Locale.Get("server_broken"), Locale.Get("sync"), MessageBoxButton.OK, MessageBoxImage.Error);
                Title = Locale.Get("window_title", Locale.Get("not_connected"));
                return;
            }
            if (version == App.Current.Settings.Version)
            {
                WindowBlocker.Visibility = Visibility.Hidden;
                MessageBox.Show(Locale.Get("sync_up_to_date"), Locale.Get("sync"));
                Title = Locale.Get("window_title", Locale.Get("connected", GetCurrentTimeHM()));
                return;
            }
            await Files.SaveAll(false);
            SyncResult syncResult = await App.Current.SyncService.EditorSync();
            if (syncResult == SyncResult.ServerUnavailable)
            {
                WindowBlocker.Visibility = Visibility.Hidden;
                MessageBox.Show(Locale.Get("server_unreachable"), Locale.Get("sync"), MessageBoxButton.OK, MessageBoxImage.Error);
                Title = Locale.Get("window_title", Locale.Get("not_connected"));
                return;
            }
            if (syncResult == SyncResult.Other)
            {
                WindowBlocker.Visibility = Visibility.Hidden;
                MessageBox.Show(Locale.Get("server_update_fail"), Locale.Get("sync"), MessageBoxButton.OK, MessageBoxImage.Error);
                Title = Locale.Get("window_title", Locale.Get("not_connected"));
                return;
            }
            App.Current.Settings.UpdateVersion(version);
            WindowBlocker.Visibility = Visibility.Hidden;
            MessageBox.Show(Locale.Get("synced"), Locale.Get("sync"));
            Title = Locale.Get("window_title", Locale.Get("connected", GetCurrentTimeHM()));
            await Reload();
        }

        public async Task Commit()
        {
            if (Files.FilesList.SelectedItem is not FileTab tab) return;
            if (!tab.HasApprovedChanges) return;
            WindowBlocker.Visibility = Visibility.Visible;
            int version = await App.Current.SyncService.CompareVersions();
            if (version == -1)
            {
                WindowBlocker.Visibility = Visibility.Hidden;
                MessageBox.Show(Locale.Get("server_unreachable"), Locale.Get("sync"), MessageBoxButton.OK, MessageBoxImage.Error);
                Title = Locale.Get("window_title", Locale.Get("not_connected"));
                return;
            }
            if (version == -2)
            {
                WindowBlocker.Visibility = Visibility.Hidden;
                MessageBox.Show(Locale.Get("server_broken"), Locale.Get("sync"), MessageBoxButton.OK, MessageBoxImage.Error);
                Title = Locale.Get("window_title", Locale.Get("not_connected"));
                return;
            }
            if (version > App.Current.Settings.Version)
            {
                WindowBlocker.Visibility = Visibility.Hidden;
                MessageBox.Show(Locale.Get("update_first"), Locale.Get("sync"), MessageBoxButton.OK, MessageBoxImage.Warning);
                Title = Locale.Get("window_title", Locale.Get("not_connected"));
                return;
            }
            if (!await App.Current.SyncService.Commit(tab))
            {
                WindowBlocker.Visibility = Visibility.Hidden;
                MessageBox.Show(Locale.Get("commit_fail"), Locale.Get("sync"), MessageBoxButton.OK, MessageBoxImage.Error);
                Title = Locale.Get("window_title", Locale.Get("not_connected"));
                return;
            }
            await App.Current.LocalFiles.ApplyApprovedChanges(tab);
            WindowBlocker.Visibility = Visibility.Hidden;
            MessageBox.Show(Locale.Get("synced"), Locale.Get("sync"));
            Title = Locale.Get("window_title", Locale.Get("connected", GetCurrentTimeHM()));
            await Reload();
        }

        private async Task Reload()
        {
            string tabName = "";
            string entryId = "";
            if (Files.FilesList.SelectedItem is FileTab tab)
            {
                tabName = tab.RelativePath;
            }
            if (TranslationGrid.TGrid.SelectedItem is StringEntry entry)
            {
                entryId = entry.Id;
            }
            Files.Files.Clear();
            FileTab? snapTo = null;
            foreach (var file in await App.Current.LocalFiles.ReadLocalFiles())
            {
                Files.Files.Add(file);
                if (file.RelativePath == tabName)
                {
                    file.SelectedEntry = entryId;
                    snapTo = file;
                }
            }
            Files.FilesView?.Refresh();
            if (snapTo == null)
            {
                Files.FilesList.SelectedIndex = 0;
                return;
            }
            Files.FilesList.SelectedItem = snapTo;
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Buttons.SearchBox.Focus();
                Buttons.SearchBox.SelectAll();
                e.Handled = true;
            }
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (!Buttons.SaveFile.IsEnabled) return;
                Buttons.SaveAll.IsEnabled = false;
                Buttons.SaveFile.IsEnabled = false;
                if (Files.SaveFile())
                {
                    await Buttons.ShowStatusAsync(Locale.Get("changes_saved"), true);
                }
                Buttons.SaveAll.IsEnabled = true;
                Buttons.SaveFile.IsEnabled = true;
            }
            if (e.Key == Key.S && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (!Buttons.SaveAll.IsEnabled) return;
                Buttons.SaveAll.IsEnabled = false;
                Buttons.SaveFile.IsEnabled = false;
                if (await Files.SaveAll(true))
                {
                    await Buttons.ShowStatusAsync(Locale.Get("changes_saved"), false);
                }
                Buttons.SaveAll.IsEnabled = true;
                Buttons.SaveFile.IsEnabled = true;
            }
            if (e.Key == Key.Escape)
            {
                TranslationGrid.TGrid.UnselectAll();
            }
        }

    }
}