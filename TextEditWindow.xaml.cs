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
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
