using Chris.BaseClasses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace Chris.VoiceProviders {
    internal class YoutubeVoiceProvider {
        private static YouTubeService youtubeService;
        private static SearchResource.ListRequest searchListRequest;
        private static YoutubeClient client = new YoutubeClient();
        private static readonly ILog log;
        private IList<SearchResult> result = null;
        private int selection = -1;

        public string CurrentSelection {
            get {
                if (result != null || selection >= 0 || selection < result.Count) {
                    return result[selection].Snippet.Title;
                }
                return "no selection";
            }
        }

        public List<string> Result {
            get {
                if (result != null) {
                    List<string> r = new List<string>();
                    foreach(SearchResult s in result) {
                        r.Add(s.Snippet.Title);
                    }
                    return r;
                }
                return new List<string> { "no selection" };
            }
        }

        static YoutubeVoiceProvider() {
            log = LogManager.GetLogger("Azuki", typeof(YoutubeVoiceProvider).Name);
            youtubeService = new YouTubeService(new BaseClientService.Initializer() {
                ApiKey = "AIzaSyDuqzRCRHAiInfib5ENRbtIRGmdl2gkUes",
                ApplicationName = "Azuki"
            });
            searchListRequest = youtubeService.Search.List("snippet");
        }

        public async Task<IList<SearchResult>> Search(string query, int limit = 1) {
            searchListRequest.Q = query;
            searchListRequest.MaxResults = limit;
            searchListRequest.Type = "video";
            SearchListResponse searchListResponse = await searchListRequest.ExecuteAsync();
            return searchListResponse.Items;
        }

        public void DownloadAudio(string youtubeid, string path) {
            try {
                Task<MediaStreamInfoSet> t = client.GetVideoMediaStreamInfosAsync(youtubeid);
                t.Wait();
                MediaStreamInfoSet infoSet = t.Result;
                AudioStreamInfo audio = infoSet.Audio.WithHighestBitrate();
                Task t2 = client.DownloadMediaStreamAsync(audio, path);
                t2.Wait();
            } catch (Exception e) {
                log.Warn(e.Message + "\n" + e.InnerException);
            }
        }

        internal List<string> DoQuery(string query) {
            Task<IList<SearchResult>> t = Search(query, 5);
            t.Wait();
            result = t.Result;
            List<string> ret = new List<string>();
            foreach(SearchResult r in result) {
                ret.Add(r.Snippet.Title);
            }
            return ret;
        }

        internal void SelectResult(int id) {
            if (result == null || id <= 0 || id > result.Count) {
                log.Warn("No result to select from/ wrong selection:" + id);
                return;
            }
            selection = id - 1;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal async Task DownloadToFileByQuery(string path) {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            if (result == null || selection < 0 || selection > result.Count) {
                log.Warn("No result to select from/ wrong selection:" + selection);
                return;
            }
            DownloadAudio(result[selection].Id.VideoId, path);
        }
        public async Task<string> Link(string path)
        {
            var infoset = await client.GetVideoMediaStreamInfosAsync(path);
            var video = await client.GetVideoAsync(path);
            Console.WriteLine("Getting link for: " + video.Title + "\nFrom channel: " + video.Author);
            var info = infoset.Muxed.WithHighestVideoQuality();
            return info.Url;
        }
    }
}
