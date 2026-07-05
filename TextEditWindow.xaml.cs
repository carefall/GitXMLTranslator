
using System.Windows;

namespace GitXMLTranslator
{
    /// <summary>
    /// Логика взаимодействия для TextEditWindow.xaml
    /// </summary>
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
