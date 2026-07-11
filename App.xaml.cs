using RestXMLTranslator.Internals.Program;
using RestXMLTranslator.Internals.Services;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Xml;
using Windows.Globalization;

namespace RestXMLTranslator
{
    public partial class App : Application
    {

        static App()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Logger.Setup();
        }

        public static new App Current => (App)Application.Current;

        public MainWindow MWindow => (MainWindow)Application.Current.MainWindow;

        public readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public readonly XmlWriterSettings XmlSettings = new()
        {
            Encoding = Encoding.GetEncoding(1251),
            Indent = true
        };

        public LocalFileService LocalFiles { get; } = new();
        public SyncService SyncService { get; } = new();
        public Settings Settings { get; }

        public App()
        {
            Settings = new Settings();
            if (string.IsNullOrEmpty(Settings.Language))
            {
                Settings.SelectLanguage();
            }
            Locale.Init(Settings.Language == "eng");
            if (string.IsNullOrWhiteSpace(Settings.GameDataPath))
            {
                Settings.SelectGameDataFolder();
            }
            new StartupWindow().Show();
        }

        public void SwitchTheme()
        {
            Settings.SwitchTheme();
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var oldTheme = dictionaries.FirstOrDefault(d => d.Source != null &&
            (d.Source.OriginalString.Contains("DarkTheme.xaml")
            || d.Source.OriginalString.Contains("LightTheme.xaml")));
            if (oldTheme != null)
                dictionaries.Remove(oldTheme);
            string path = Settings.LightTheme ? "Themes/LightTheme.xaml" : "Themes/DarkTheme.xaml";
            dictionaries.Insert(0, new ResourceDictionary
            {
                Source = new Uri(path, UriKind.Relative)
            });
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ApplyTheme();
        }
    }
}
