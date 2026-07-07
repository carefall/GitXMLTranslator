using RestXMLTranslator.Internals;
using System.Windows;

namespace RestXMLTranslator
{
    public partial class TextEditWindow : Window
    {
        public string ResultText => Editor.Text;

        public TextEditWindow(string text)
        {
            InitializeComponent();
            Editor.Text = text;
            Editor.Focus();
            Editor.CaretIndex = Editor.Text.Length;
            Title = Locale.Get("enter_text");
            Cancel.Content = Locale.Get("btn_cancel");
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
