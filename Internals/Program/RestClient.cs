using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace RestXMLTranslator.Internals.Program
{
    public static class RestClient
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        static RestClient()
        {
            Client.DefaultRequestHeaders.Add(
                "X-App-Version",
                Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0"
            );
        }

        private const string BaseUrl = "https://nukerfall.pythonanywhere.com/translator/"; //"https://nukerfall.pythonanywhere.com/translator/"; "http://127.0.0.1:8000/translator/";

        public static async Task<string> GetDataAsync(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await Client.GetAsync($"{BaseUrl}{endpoint}");
                string json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.MethodNotAllowed) return "0";
                if (response.StatusCode == HttpStatusCode.NotImplemented) return "1";
                if (!response.IsSuccessStatusCode) throw new Exception(json);
                return json;
            }
            catch (Exception ex)
            {
                Logger.Log($"Unable to sync data with server. Exception: {ex}", "RestClient-Get");
                return "";
            }
        }

        public static async Task<string> PostDataAsync(string endpoint, string body)
        {
            try
            {
                using var content = new StringContent(body, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await Client.PostAsync($"{BaseUrl}{endpoint}", content);
                string json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.MethodNotAllowed) return "0";
                if (response.StatusCode == HttpStatusCode.NotImplemented) return "1";
                if (!response.IsSuccessStatusCode) throw new Exception(json);
                return json;
            }
            catch (Exception ex)
            {
                Logger.Log($"Unable to sync data with server. Exception: {ex}", "RestClient-Post");
                return "";
            }
        }

    }
}
