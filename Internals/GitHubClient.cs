using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Windows;

namespace GitXMLTranslator.Internals
{
    internal class GitHubClient
    {

        public static async Task<int> Check(string gameDataPath)
        {
            var hashesDict = await GetGitHubHashes("carefall", "GitXMLTranslator", "Assets/hashes.json");
            if (hashesDict == null)
            {
                MessageBox.Show("Соединение с сервером не установлено.", "Синхронизация", MessageBoxButton.OK);
                return -1;
            }
            var localJsons = GetLocalFiles(gameDataPath + "/gamedata/configs", true);
            if (localJsons == null)
            {
                MessageBox.Show("Ошибка проверки файлов на диске.", "Синхронизация", MessageBoxButton.OK);
                return -1;
            }
            List<string> filesToDelete = new();
            Dictionary<string, string> filesToUpdate = new();
            foreach (string localFile in localJsons)
            {
                string path = localFile;
                if (!hashesDict.ContainsKey(localFile))
                {
                    filesToDelete.Add(path);
                    continue;
                }
                if (hashesDict[localFile] != ComputeFileSha1(path)) filesToUpdate[localFile] = path;
            }
            foreach (string gitFile in hashesDict.Keys)
            {
                string path = Path.Combine(gameDataPath + "/gamedata/configs", gitFile);
                if (!localJsons.Contains(gitFile)) filesToUpdate[gitFile] = path;
            }
            if (filesToUpdate.Count > 0 || filesToDelete.Count > 0)
            {
                string filesToLoad = filesToUpdate.Count > 0 ? string.Join(" ", filesToUpdate.Keys.ToArray()) : "none";
                string filesToDel = filesToDelete.Count > 0 ? string.Join(" ", filesToDelete) : "none";
                Logger.Log("Updater", $"Found new version. Files to load from source: {filesToLoad}. Files to delete: {filesToDel}.");
                Logger.Log("Updater", "Installing update.");
                var keys = filesToUpdate.Keys.ToArray();
                int len = keys.Length;
                for (int i = 0; i < len; i++)
                {
                    var key = keys[i];
                    await DownloadFile(key, "carefall", "GitXMLTranslator");
                }
                foreach (var path in filesToDelete)
                {
                    MessageBox.Show($"Файл {path} будет удалён из основной ветви и отправлен в бэкапы.", "Синхронизация", MessageBoxButton.OK);
                    string text = File.ReadAllText(path);
                    File.WriteAllText(path.Replace("Assets", "Backups"), text);
                    File.Delete(path);
                }
                return 1;
            }
            return 0;
        }

        private static async Task<Dictionary<string, string>?> GetGitHubHashes(string owner, string repo, string path)
        {
            string url = $"https://raw.githubusercontent.com/{owner}/{repo}/main/{path}";
            using HttpClient client = new()
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("GitXMLTranslator");
            try
            {
                string json = await client.GetStringAsync(url);
                JObject obj = JObject.Parse(json);
                var hashesDict = obj.ToObject<Dictionary<string, string>>();
                return hashesDict;
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                return null;
            }
        }

        public static List<string>? GetLocalFiles(string folderPath, bool json)
        {
            try
            {
                return [.. Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Select(f => {
                    var relative = Path.GetRelativePath(folderPath, f);
                    return relative.Replace("\\", "/");
                })];
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                return null;
            }
        }

        public static string ComputeFileSha1(string filePath)
        {
            using var sha1 = SHA1.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha1.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public static async Task DownloadFile(string file, string owner, string repo)
        {
            try
            {
                using var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };
                MessageBox.Show($"https://raw.githubusercontent.com/{owner}/{repo}/main/gamedata/configs/{file}");
                var data = await client.GetByteArrayAsync($"https://raw.githubusercontent.com/{owner}/{repo}/main/Assets/{file}");
                var fileFolders = file.Split('/');
                var fileFolderWithoutFile = fileFolders[0];
                for (int i = 1; i < fileFolders.Length - 1; i++)
                {
                    fileFolderWithoutFile += "/" + fileFolders[i];
                }
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/Downloads/" + fileFolderWithoutFile);
                await File.WriteAllBytesAsync(AppDomain.CurrentDomain.BaseDirectory + "/Downloads/" + file, data);
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                MessageBox.Show("Не удалось загрузить и записать файл с GitHub.", "Синхронизация", MessageBoxButton.OK);
            }
        }
    }
}
