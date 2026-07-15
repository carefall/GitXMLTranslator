using RestXMLTranslator.Internals.Models;
using System.Windows;

namespace RestXMLTranslator
{
    public partial class TranslationSelectWindow : Window
    {


        public ImportType Result { get; private set; }

        public TranslationSelectWindow()
        {
            InitializeComponent();
        }

        private void Select(ImportType type)
        {
            Result = type;
            DialogResult = true;
            Close();
        }

        private void Russian_Click(object sender, RoutedEventArgs e) => Select(ImportType.Russian);

        private void English_Click(object sender, RoutedEventArgs e) => Select(ImportType.English);

        private void Localization_Click(object sender, RoutedEventArgs e) => Select(ImportType.Localization);

        private void Comments_Click(object sender, RoutedEventArgs e) => Select(ImportType.Comments);

        private void All_Click(object sender, RoutedEventArgs e) => Select(ImportType.All);
    }
}
