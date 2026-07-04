using GitXMLTranslator.Internals;
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
                StartUpdate();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settings.UpdateName(Name.Text);
            StartUpdate();
        }

        private async void StartUpdate()
        {
            ContinueButton.IsEnabled = false;
            int result = await GitHubClient.Check(settings.gamedataPath);
            if (result == -1)
            {
                Application.Current.Shutdown();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
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
