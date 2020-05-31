using Azuki.Core.Modules.Api;

namespace Azuki.Core.Modules {
    public class MusicPlayerModule : BaseModule {
        /*private static readonly List<string> Tempnamelist = new List<string> { "Temp0", "Temp1", "Temp2", "Temp3", "Temp4", "Temp5", "Temp6", "Temp7" };
        private static int i = 8;
        private CancellationTokenSource playCancellationTokenSource = new CancellationTokenSource();
        public ManualResetEvent stopsignal = new ManualResetEvent(false);
        private VoiceProvider voice;

        [Command(NeedsHandler = true, NeedsMessage = true, NeedsVoice = true, HasParams = true)]
        public async void Play(ICoreHandler handler, Message message) {
            DiscordGuildTextChannel Channel = client.GetChannel(e.Message.ChannelId).Result as DiscordGuildTextChannel;
            Snowflake GuildId = Channel.GuildId;
            DiscordGuild Guild = await client.GetGuild(GuildId);
            Snowflake ChannelId = e.Message.ChannelId;
            DiscordVoiceState voiceState = e.Shard.Cache.GetVoiceState(GuildId, e.Message.Author.Id);
            if (voiceState == null) {
                return;
            } else {
                if (!SharedObjectStorage.VoiceModuleObjects.Keys.Contains(voiceState.ChannelId.Value)) {
                    VoiceModule music = new VoiceModule(e.Shard, voiceState.ChannelId.Value);
                    SharedObjectStorage.VoiceModuleObjects.Add(voiceState.ChannelId.Value, music);
                    if (Tempnamelist.Count <= 2) {
                        Tempnamelist.Add($"Temp{i++}");
                    }
                    string filename = Tempnamelist[0];
                    Tempnamelist.RemoveAt(0);
                    voice = new YoutubeVoiceProvider();
                    voice.DoQuery(query);
                    client.CreateMessage(e.Message.ChannelId, "" + String.Join("\n", voice.Result));
                    this.GuildId = ((await client.GetChannel(e.Message.ChannelId)) as DiscordGuildTextChannel).GuildId;
                    UserId = e.Message.Author.Id;
                    e.Shard.Gateway.OnMessageCreated += Gateway_OnMessageCreated1;
                    stopsignal.WaitOne();
                    e.Shard.Gateway.OnMessageCreated -= Gateway_OnMessageCreated1;
                    voice.DownloadToFileByQuery($"Temp/{filename}").Wait();
                    if (new FileInfo($"Temp/{filename}").Length <= 100) { return; }
                    client.CreateMessage(e.Message.ChannelId, "Playing: " + voice.CurrentSelection);
                    Converter c = new FFmpegConverter();
                    c.TempfileClosed += TempfileClosed;
                    music.Transfer(new FileInfo($"Temp/{filename}"), c, playCancellationTokenSource);
                }
            }
        }
        /*private void TempfileClosed(object sender, FileSystemEventArgs e) {
            try {
                log.Debug($"Deleting {e.FullPath}");
                File.Delete(e.FullPath);
                Tempnamelist.Add(e.Name);
            } catch (Exception ex) {
                log.Warn(ex.Message);
            }
        }*/
    }
}
