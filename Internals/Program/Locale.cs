using System.IO;
using System.Windows;
using System.Text.Json;

namespace RestXMLTranslator.Internals.Program
{
    internal static class Locale
    {

        private static IReadOnlyDictionary<string, string> locales = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static bool Init(bool english)
        {
            try
            {
                var file = english ? "locale_en.json" : "locale_ru.json";
                string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file));
                locales = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? throw new Exception($"Deserialization of {file} failed...");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), "LocaleLoader");
                MessageBox.Show(english ? $"Error! Locale file not found!" : "Ошибка! Не найден файл локализации!", "RestXMLTranslator", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
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
