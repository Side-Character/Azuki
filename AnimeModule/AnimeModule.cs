using AzukiModuleApi;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace AnimeModule {
    public class AnimeModule : BaseModule {
        private static HttpClient client = new HttpClient();
        public AnimeModule() { }
        public async void Search(ICoreHandler handler, ulong channelid, string @params) {
            int number = 5;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string data = (await client.GetStringAsync("https://www.anime-planet.com/anime/all?name=" + WebUtility.HtmlEncode(@params)));
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(data);
            HtmlNode document = html.DocumentNode;
            for (int i = 0; i < number; i++) {
                HtmlNode info = document.QuerySelectorAll("h2.cardName").ToArray()[i];
                handler.Respond(channelid, info.InnerText);
            }
        }
    }
}
