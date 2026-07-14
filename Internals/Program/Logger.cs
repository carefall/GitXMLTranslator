using System.IO;
using System.Windows;

namespace RestXMLTranslator.Internals.Program
{
    internal static class Logger
    {

        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
        private static readonly Lock Lock = new();

        public static void Log(string message, string thrower = "TEST")
        {
            if (!File.Exists(LogPath)) return;
            try
            {
                lock (Lock)
                {
                    using StreamWriter writer = new(LogPath, true);
                    writer.WriteLine($"[{DateTime.Now}] [{thrower}]: {message}");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось записать информацию в файл log.txt.\nUnable to write data to log.txt file.", "Логирование / Logging", MessageBoxButton.OK);
            }
        }

        public static void Setup()
        {
            try
            {
                if (!File.Exists(LogPath)) File.Create("log.txt").Close();
                Log("Logging initialized", "Logger");
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось записать информацию в файл log.txt.\nUnable to write data to log.txt file.", "Логирование / Logging", MessageBoxButton.OK);
            }
        }
    }
}
