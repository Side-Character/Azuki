using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chris.Utils {
    public class Downloader {
        private static HttpClient client = new HttpClient();

        public static async void DownloadToFile(string url, string path) {
            Stream stream = await client.GetStreamAsync(url);
            using (StreamWriter sw = new StreamWriter(path)) {
                stream.CopyTo(sw.BaseStream);
            }
        }

        public static async Task<string> DownloadToString(string url) {
            client.CancelPendingRequests();
            return await client.GetStringAsync(url);
        }
    }
}
