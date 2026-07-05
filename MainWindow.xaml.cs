using System.Windows;

namespace GitXMLTranslator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string name;

        public MainWindow(string name)
        {
            InitializeComponent();
            this.name = name;
        }
    }
}