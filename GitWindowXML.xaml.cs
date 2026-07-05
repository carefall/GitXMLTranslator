using GitXMLTranslator.Internals;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace GitXMLTranslator
{

    public partial class GitWindowXML : Window
    {

        private static readonly Regex _regex = new(@"^[\p{L}\p{Nd}\s.,-]*$", RegexOptions.Compiled);
        private Settings settings;

        public GitWindowXML()
        {
            InitializeComponent();
            Logger.Setup();
            settings = new();
            if (settings.name != "")
            {
                ContinueButton.Visibility = Visibility.Hidden;
                NameBox.Visibility = Visibility.Hidden;
                Text.Text = $"Добро пожаловать, {settings.name}. Идёт синхронизация.";
                StartUpdate();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settings.UpdateName(NameBox.Text);
            StartUpdate();
        }

        private async void StartUpdate()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory + "/Downloads";
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive: true);
                Directory.CreateDirectory(dir);
            }
            ContinueButton.IsEnabled = false;
            int result = await GitHubClient.Check(settings.gamedataPath);
            if (result == -1)
            {
                Application.Current.Shutdown();
                return;
            }
            if (result == 0)
            {
                new MainWindow(settings.name).Show();
                Close();
                return;
            }
            var window = new ConflictWindow(settings.name, settings.gamedataPath);
            window.Show();
            window.RebuildLocalFiles();
            Close();
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;
            if (!_regex.IsMatch(textBox.Text))
            {
                int caret = textBox.CaretIndex;
                if (caret > 0)
                {
                    textBox.Text = textBox.Text.Remove(caret - 1, 1);
                    textBox.CaretIndex = caret - 1;
                }
            }
            ContinueButton.IsEnabled = textBox.Text.Length > 2;



        }
    }
}
