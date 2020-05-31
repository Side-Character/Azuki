using Azuki.Core.Modules.Api;
using FFmpeg.AutoGen;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Azuki.Core.Modules {
    public class MusicPlayerModule : BaseModule {
        private static readonly YoutubeClient client = new YoutubeClient();

        [Command(NeedsHandler = true, NeedsMessage = true, NeedsVoice = true, HasParams = true)]
        public async Task Play(ICoreHandler handler, Message message, string param) {
            Video vid = (await client.Search.GetVideosAsync(param)).FirstOrDefault();
            if (vid != null) {
                if (vid.Duration <= TimeSpan.FromMinutes(10)) {
                    StreamManifest streamManifest = await client.Videos.Streams.GetManifestAsync(vid.Id);
                    AudioOnlyStreamInfo audio = streamManifest.GetAudioOnly().FirstOrDefault();
                    if (audio != null) {
                        MemoryStream stream = new MemoryStream();
                        await client.Videos.Streams.CopyToAsync(audio, stream);
                        Debug(audio.AudioCodec);
                        unsafe {
                            AVPacket* pkt = ffmpeg.av_packet_alloc();
                            AVCodec* codec = ffmpeg.avcodec_find_decoder_by_name(audio.AudioCodec);
                            if (codec == null) {
                                Error($"Codec {audio.AudioCodec} not found.");
                                return;
                            }
                            AVCodecParserContext* parser = ffmpeg.av_parser_init((int)codec->id);
                            if (parser == null) {
                                Error("Could not allocate audio codec context.");
                                return;
                            }
                            AVCodecContext* context = ffmpeg.avcodec_alloc_context3(codec);
                            if (context == null) {
                                Error("Could not allocate audio codec context.");
                                return;
                            }
                            if (ffmpeg.avcodec_open2(context, codec, null) < 0) {
                                Error("Could not open audio codec context.");
                                return;
                            }
                            AVFrame* decoded_frame = null;
                            while (stream.Length - stream.Position > 0) {
                                if (decoded_frame == null) {
                                    decoded_frame = ffmpeg.av_frame_alloc();
                                }
                                byte[] buffer = new byte[pkt->size];
                                stream.Read(buffer, 0, buffer.Length);
                                IntPtr unmanagedPointer = Marshal.AllocHGlobal(buffer.Length);
                                Marshal.Copy(buffer, 0, unmanagedPointer, buffer.Length);
                                ffmpeg.av_parser_parse2(parser, context, &pkt->data, &pkt->size, (byte*)unmanagedPointer, buffer.Length, ffmpeg.AV_NOPTS_VALUE, ffmpeg.AV_NOPTS_VALUE, 0);
                                int ret = ffmpeg.avcodec_send_packet(context, pkt);
                                while(ret > 0) {
                                    ret = ffmpeg.avcodec_receive_frame(context, decoded_frame);
                                    int data_size = ffmpeg.av_get_bytes_per_sample(context->sample_fmt);
                                    int current = 0;
                                    for (int i = 0; i < decoded_frame->nb_samples; i++) {
                                        for (uint ch = 0; ch < context->channels; ch++) {
                                            Marshal.Copy((IntPtr)decoded_frame->data[ch] + (data_size * i), buffer, current, data_size);
                                            current += data_size;
                                        }
                                    }
                                    //message.Tr
                                    //handler. .SendVoiceData(buffer, 0, read);
                                }
                                Marshal.FreeHGlobal(unmanagedPointer);
                            }
                        }
                    }
                } else {
                    Warn("Video too long.");
                }
            } else {
                Warn("No video by that term.");
            }
            /*DiscordGuildTextChannel Channel = client.GetChannel(e.Message.ChannelId).Result as DiscordGuildTextChannel;
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
            }*/
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
