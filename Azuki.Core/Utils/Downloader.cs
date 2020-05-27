using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azuki.Utils {
    public static class Downloader {
        private static HttpClient client = new HttpClient();

        public static async void DownloadToFile(Uri url, string path) {
            Stream stream = await client.GetStreamAsync(url).ConfigureAwait(true);
            using (StreamWriter sw = new StreamWriter(path)) {
                stream.CopyTo(sw.BaseStream);
            }
        }
        public static async void DownloadToFile(string url, string path) {
            Stream stream = await client.GetStreamAsync(new Uri(url)).ConfigureAwait(true);
            using (StreamWriter sw = new StreamWriter(path)) {
                stream.CopyTo(sw.BaseStream);
            }
        }
        public static async Task<string> DownloadToString(Uri url) {
            client.CancelPendingRequests();
            return await client.GetStringAsync(url).ConfigureAwait(true);
        }
        public static async Task<string> DownloadToString(string url) {
            client.CancelPendingRequests();
            return await client.GetStringAsync(new Uri(url)).ConfigureAwait(true);
        }
    }
}
