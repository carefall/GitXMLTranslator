using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace RestXMLTranslator.Internals
{
    internal static class Locale
    {

        private static Dictionary<string, string> locales = new();

        public static void Init()
        {
            try
            {
                string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "locale.json");
                if (json == null)
                {
                    Logger.Log("LocaleLoader", "No locale file found!");
                    MessageBox.Show("Error! No locale found!\nОшибка! Не найдена локализация!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.Current.Shutdown();
                    return;
                }
                locales = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;
            }
            catch (Exception ex)
            {
                Logger.Log("LocaleLoader", ex.ToString());
                MessageBox.Show("Error! No locale found!\nОшибка! Не найдена локализация!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
            }
        }

        public static string Get(string key)
        {
            return locales.GetValueOrDefault(key, $"{key}");
        }

        internal static string Get(string key, string val)
        {
            return locales.GetValueOrDefault(key, $"{key}").Replace("%value%", val);
        }
    }
}
