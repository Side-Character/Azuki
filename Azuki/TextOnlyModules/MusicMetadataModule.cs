using Discore;
using Discore.WebSocket;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System.Net.Http;

namespace Chris.TextOnlyModules {
    internal class MusicMetadataModule {

        protected void AddHooks(IDiscordGateway gateway) { }

        public async void Quote(DiscordMessage Message, string input) {
            /*string url = "https://search.azlyrics.com/search.php?q=" + input.Replace("-", "+");
            HttpClient search = new HttpClient();
            string response = await search.GetStringAsync(url);
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(response);
            HtmlNode document = html.DocumentNode;
            HtmlNode list = document.QuerySelector(".text-left");
            string song = list.ChildNodes[1].ChildNodes[1].ChildNodes[0].InnerText;
            string artist = list.ChildNodes[1].ChildNodes[3].ChildNodes[0].InnerText;
            client.CreateMessage(Message.ChannelId, artist + "\n" + song);
            url = list.ChildNodes[1].ChildNodes[1].ChildNodes[0].GetAttributeValue("href", string.Empty);
            response = await search.GetStringAsync(url);
            html.LoadHtml(response);
            document = html.DocumentNode;*/
        }
    }
}
