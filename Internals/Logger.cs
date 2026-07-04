using System.IO;
using System.Windows;

namespace GitXMLTranslator.Internals
{
    internal class Logger
    {

        public static void Log(string thrower, string message)
        {
            if (!File.Exists("log.txt")) return;
            try
            {
                using StreamWriter writer = new("log.txt", true);
                writer.WriteLine($"[{DateTime.Now}] [{thrower}]: {message}");
            } catch (Exception)
            {
                MessageBox.Show("Не удалось записать информацию в файл лога. Это очень плохо ;(", "Логирование", MessageBoxButton.OK);
            }
        }

        internal static void Setup()
        {
            if (File.Exists("log.txt")) return;
            try
            {
                File.Create("log.txt").Close();
            } catch (Exception)
            {
                MessageBox.Show("Не удалось создать файл лога. Это очень плохо ;(", "Запуск", MessageBoxButton.OK);
            }
        }
    }
}
