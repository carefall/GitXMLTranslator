using RestXMLTranslator.Internals;
using System.Windows;

namespace RestXMLTranslator
{
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