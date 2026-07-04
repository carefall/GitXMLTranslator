using GitXMLTranslator.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace GitXMLTranslator
{

    public partial class GitWindowXML : Window
    {

        private static readonly Regex _regex = new(@"^[\p{L}\p{Nd}\s.,-]*$", RegexOptions.Compiled);

        public GitWindowXML()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ContinueButton.IsEnabled = false;
            Settings s = new();
            int result = await GitHubClient.Check(s.gamedataPath);
            //
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
