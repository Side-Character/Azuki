using Discore;
using Discore.Voice;
using Discore.WebSocket;
using log4net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Chris.BaseClasses {
    public class VoiceModule {
        /*protected Shard shard;
        protected DiscordGuild guild;
        protected DiscordVoiceConnection connection;
        protected DiscordGuildVoiceChannel channel;
        protected readonly ILog log;
        private CancellationTokenSource playCancellationTokenSource;

        private VoiceModule() {
            log = LogManager.GetLogger("Azuki", GetType().Name);
        }

        public VoiceModule(Shard shard, DiscordGuild guild, DiscordGuildVoiceChannel voiceChannel) : this() {
            this.shard = shard;
            this.guild = guild;
            channel = voiceChannel;
            InitConnection();
        }

        public VoiceModule(Shard shard, Snowflake voiceChannelid) : this() {
            this.shard = shard;
            Initialize(voiceChannelid);
        }

        private async void Initialize(Snowflake voiceChannelId) {
            //channel = await SharedObjectStorage.client.GetChannel(voiceChannelId) as DiscordGuildVoiceChannel;
            //guild = await SharedObjectStorage.client.GetGuild(channel.GuildId);
            log.Debug("Module initializing for channel " + guild.Name + "." + channel.Name);
            InitConnection();
            //connection.OnInvalidated += Connection_OnInvalidated;
            log.Debug("Module ready for work on channel " + guild.Name + "." + channel.Name);
        }

        private async void InitConnection() {
            connection = shard.Voice.CreateOrGetConnection(channel.GuildId);
            await connection.ConnectAsync(channel.Id);
            while (connection.IsConnected != true) {
                await connection.ConnectAsync(channel.Id);
                Thread.Sleep(1);
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal async Task Transfer(Stream stream, CancellationTokenSource playCancellationTokenSource) {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            this.playCancellationTokenSource = playCancellationTokenSource;
            while (connection.IsConnected != true) {
                Thread.Sleep(1);
            }
            if (connection.BytesToSend > 0) {
                connection.ClearVoiceBuffer();
            }
            byte[] transferBuffer = new byte[DiscordVoiceConnection.PCM_BLOCK_SIZE];
            if (!connection.IsSpeaking) {
                connection.SetSpeakingAsync(true).Wait();
            }
            while (!playCancellationTokenSource.IsCancellationRequested && connection.IsValid && stream.CanRead) {
                // Check if there is room in the voice buffer
                if (connection.CanSendVoiceData(connection.BytesToSend + transferBuffer.Length)) {
                    // Read some voice data into our transfer buffer.
                    int read = stream.Read(transferBuffer, 0, transferBuffer.Length);
                    // Send the data we read from the source into the voice buffer.
                    connection.SendVoiceData(transferBuffer, 0, read);
                } else {
                    // Wait for at least 1ms to avoid burning CPU cycles.
                    Thread.Sleep(1);
                }
            }
            if (connection.IsSpeaking) {
                connection.SetSpeakingAsync(false).Wait();
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal async void Transfer(FileInfo file, Converter converter, CancellationTokenSource playCancellationTokenSource) {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            Stream stream = converter.Convert(file.FullName, playCancellationTokenSource);
            await Transfer(stream, playCancellationTokenSource);
            stream.Close();
        }*/
    }
}
