using System.IO;
using System.Windows;

namespace RestXMLTranslator.Internals
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
                MessageBox.Show("Не удалось записать информацию в файл log.txt.\nUnable to write data to log.txt file.", "Логирование / Logging", MessageBoxButton.OK);
            }
        }

        internal static void Setup()
        {
            if (File.Exists("log.txt")) return;
            try
            {
                File.Create("log.txt").Close();
                Log("Logger", "Logging initialized");
            } catch (Exception)
            {
                MessageBox.Show("Не удалось записать информацию в файл log.txt.\nUnable to write data to log.txt file.", "Логирование / Logging", MessageBoxButton.OK);
            }
        }
    }
}
