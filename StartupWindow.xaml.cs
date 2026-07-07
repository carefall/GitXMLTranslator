using RestXMLTranslator.Internals;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace RestXMLTranslator
{
    public partial class StartupWindow : Window
    {

        private static readonly Regex _regex = new(@"^[\p{L}\p{Nd}\s.,-]*$", RegexOptions.Compiled);

        public StartupWindow()
        {
            InitializeComponent();
            Logger.Setup();
            Locale.Init();
            Title = Locale.Get("window_title", Locale.Get("startup"));
            ContinueButton.Content = Locale.Get("continue");
            Settings.OnUserDeclined += Shutdown;
            MainWindow.OnShutdown += Shutdown;
            string name = Settings.GetInstance().name;
            if (name != "")
            {
                ContinueButton.Visibility = Visibility.Hidden;
                NameBox.Visibility = Visibility.Hidden;
                Text.Text = Locale.Get("welcome", name);
                Circle.Visibility = Visibility.Visible;
                StartUpdate();
            } else
            {
                Text.Text = Locale.Get("enter_name");
            }
        }

        private void Shutdown()
        {
            Settings.OnUserDeclined -= Shutdown;
            Close();
            return;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.GetInstance().UpdateName(NameBox.Text);
            StartUpdate();

        }

        private async void StartUpdate()
        {
            Logger.Log("Startup", "Performing update check...");
            Thread.Sleep(500);
            var settings = Settings.GetInstance();
            ContinueButton.IsEnabled = false;
            int result = await RestClient.Check(settings.gamedataPath, settings.version);
            if (result == -1)
            {
                Application.Current.Shutdown();
                return;
            }
            if (result == 1)
            {
                MessageBox.Show(Locale.Get("update_server_unreachable"), Locale.Get("sync"));
                Logger.Log("Startup", "Update check failed due to service unavailability. Moving to MainWindow");
                new MainWindow(false, settings.gamedataPath).Show();
            } else
            {
                Logger.Log("Startup", "Successful update check. Moving to MainWindow");
                new MainWindow(true, settings.gamedataPath).Show();
            }
            Close();
            return;
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
