using AzukiModuleApi;
using Discore;
using Discore.Http;
using Discore.Voice;
using Discore.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Chris
{
    internal class CoreHandler : ICoreHandler
    {
        private readonly ILog log;
        private readonly DiscordHttpClient client;
        private readonly Shard shard;
        private readonly Dictionary<string, Tuple<BaseModule, MethodInfo>> commands;
        internal List<DiscordGuild> Guilds { get; } = new List<DiscordGuild>();
        internal Dictionary<Snowflake, DiscordGuildTextChannel> AdminChannels { get; } = new Dictionary<Snowflake, DiscordGuildTextChannel>();
        internal Dictionary<Snowflake, DiscordGuildVoiceChannel> VoiceChannels { get; } = new Dictionary<Snowflake, DiscordGuildVoiceChannel>();
        public CoreHandler(DiscordHttpClient client, Shard shard, Dictionary<string, Tuple<BaseModule, MethodInfo>> commands)
        {
            log = LogManager.GetLogger("Azuki", "Core.Handler");
            this.client = client;
            this.shard = shard;
            this.commands = commands;
            shard.Gateway.OnGuildCreated += DiscoveredGuild;
            shard.Gateway.OnGuildAvailable += DiscoveredGuild;
            shard.Gateway.OnMessageCreated += Gateway_OnMessageCreated;
        }
        private void DiscoveredGuild(object sender, GuildEventArgs e)
        {
            log.Debug($"Discovered guild ({e.Guild.Name})");
            Guilds.Add(e.Guild);
            e.Guild.GetChannels().ContinueWith(AddChannels);
        }

        private async void AddChannels(Task<IReadOnlyList<DiscordGuildChannel>> channels)
        {
            IReadOnlyList<DiscordGuildChannel> chs = channels.Result;
            if (chs == null || chs.Count == 0)
            {
                log.Error("Tried to connect to an empty guild.");
                return;
            }
            DiscordGuild guild = null;
            try
            {
                guild = shard.Cache.GetGuild(chs.FirstOrDefault().GuildId);
                DiscordGuildCategoryChannel azukicontainer = chs.FirstOrDefault(ch => ch is DiscordGuildCategoryChannel) as DiscordGuildCategoryChannel;
                if (azukicontainer == null)
                {
                    azukicontainer = await client.CreateGuildChannel(guild.Id, new CreateGuildChannelOptions(DiscordChannelType.GuildCategory)
                    {
                        Name = "Azuki",
                        PermissionOverwrites = new List<OverwriteOptions> {
                        new OverwriteOptions(guild.OwnerId, DiscordOverwriteType.Member) {
                            Allow = DiscordPermission.Administrator,
                        }
                    }
                    }) as DiscordGuildCategoryChannel;
                    log.Info($"Created Azuki category channel in {guild.Name}");
                }
                DiscordGuildTextChannel ac = chs.OfType<DiscordGuildTextChannel>().FirstOrDefault(ch => ch.Name.ToLower() == "azuki_admin" && ch.ParentId == azukicontainer.Id) as DiscordGuildTextChannel;
                var qwe = ac.GuildId;
                if (ac != null)
                {
                    lock (AdminChannels)
                    {
                        AdminChannels.Add(ac.GuildId, ac);
                    }
                }
                else
                {
                    await client.CreateGuildChannel(guild.Id, new CreateGuildChannelOptions(DiscordChannelType.GuildText)
                    {
                        Name = "Azuki_admin",
                        PermissionOverwrites = new List<OverwriteOptions> {
                        new OverwriteOptions(guild.OwnerId, DiscordOverwriteType.Member) {
                            Allow = DiscordPermission.Administrator,
                        }
                    },
                        ParentId = azukicontainer.Id
                    });
                    log.Info($"Created admin channel in {guild.Name}");
                }
                DiscordGuildVoiceChannel vc = chs.OfType<DiscordGuildVoiceChannel>().FirstOrDefault(ch => ch.Name.ToLower() == "azuki.voice" && ch.ParentId == azukicontainer.Id);
                var asd = vc.GuildId;
                if (vc != null)
                {
                    lock (VoiceChannels)
                    {
                        VoiceChannels.Add(vc.GuildId, vc);
                    }
                }
                else
                {
                    await client.CreateGuildChannel(guild.Id, new CreateGuildChannelOptions(DiscordChannelType.GuildVoice)
                    {
                        Name = "Azuki.voice",
                        PermissionOverwrites = new List<OverwriteOptions> {
                        new OverwriteOptions(guild.OwnerId, DiscordOverwriteType.Member) {
                            Allow = DiscordPermission.Administrator,
                        }
                    },
                        ParentId = azukicontainer.Id
                    });
                    log.Info($"Created voice channel in {guild.Name}");
                }
            }
            catch (DiscordHttpApiException ex)
            {
                if (ex.ErrorCode == DiscordHttpErrorCode.MissingPermissions)
                {
                    DiscordDMChannel dMChannel = await client.CreateDM(guild.OwnerId);
                    _ = dMChannel.CreateMessage($"Please give Azuki permission te create channels. ({guild.Name})");
                }
                else
                {
                    log.Error(ex);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        internal void Respond(Snowflake channel, string message)
        {
            client.CreateMessage(channel, message);
        }
        public void Respond(ulong channelid, string message)
        {
            client.CreateMessage(new Snowflake(channelid), message);
        }
        internal void React(Snowflake channel, Snowflake message, string emojiname)
        {
            client.CreateReaction(channel, message, new DiscordReactionEmoji(emojiname));
        }
        public void React(ulong channelid, ulong messageid, string emojiname)
        {
            client.CreateReaction(new Snowflake(channelid), new Snowflake(messageid), new DiscordReactionEmoji(emojiname));
        }
        internal void EditMessage(Snowflake channel, Snowflake message, string content)
        {
            client.EditMessage(channel, message, content);
        }
        public void EditMessage(ulong channelid, ulong messageid, string content)
        {
            client.EditMessage(new Snowflake(channelid), new Snowflake(messageid), content);
        }
        protected virtual void Gateway_OnMessageCreated(object sender, MessageEventArgs e)
        {
            if (!e.Message.Author.IsBot)
            {
                if (!e.Message.Content.StartsWith(Azuki.config.DefaultStartChar))
                {
                    return;
                }
                List<string> split = e.Message.Content.Split(" ", 2).ToList();
                string command = split.FirstOrDefault().TrimStart(Azuki.config.DefaultStartChar).ToLower();
                string paramstring = "";
                if (split.Count > 1)
                {
                    paramstring = split[1];
                }
                List<KeyValuePair<string, Tuple<BaseModule, MethodInfo>>> possibleCommands = commands.Where(t => t.Key.ToLower().EndsWith(command)).ToList();
                if (Azuki.Admins.Contains(e.Message.Author.Id))
                {
                    possibleCommands.AddRange(Azuki.AdminCommands.Where(t => t.Key.ToLower().EndsWith(command)).ToList());
                }
                if (possibleCommands.Count() <= 0)
                {
                    _ = client.CreateMessage(e.Message.ChannelId, $"404 - Command not found. ({command})");
                    return;
                }
                else if (possibleCommands.Count() > 1)
                {
                    _ = client.CreateMessage(e.Message.ChannelId, "Multiple Commands found.");
                    log.Info($"Multiple Commands found. ({string.Join(',', possibleCommands)})");
                    return;
                }
                ExecuteCommand(possibleCommands.FirstOrDefault(), e, paramstring);
            }
        }

        private async void ExecuteCommand(KeyValuePair<string, Tuple<BaseModule, MethodInfo>> Command, MessageEventArgs e, string @params)
        {
            try
            {
                log.Debug("Executing command: " + Command.Key + ":" + ((@params != "") ? @params : "no params"));
                Stopwatch st = new Stopwatch();
                st.Start();
                MethodInfo method = Command.Value.Item2;
                CommandAttribute attr = method.GetCustomAttribute<CommandAttribute>();
                List<object> parameters = new List<object>();
                foreach (ParameterInfo info in method.GetParameters())
                {
                    Type t = info.ParameterType;
                    if (parameters.Exists(p => p.GetType().Equals(t)))
                    {
                        log.Error($"Duplicate parameter with type ({info.ParameterType.Name}).");
                        return;
                    }
                    switch (t.Name)
                    {
                        case "ICoreHandler":
                            {
                                if (!attr.NeedsHandler)
                                {
                                    log.Warn($"Using parameter with type ({info.ParameterType.Name}) but flagged not using.");
                                }
                                parameters.Add(this);
                                break;
                            }
                        case "Message":
                            {
                                if (!attr.NeedsMessage)
                                {
                                    log.Warn($"Using parameter with type ({info.ParameterType.Name}) but flagged not using.");
                                }
                                parameters.Add(new Message(e.Message.Id, e.Message.ChannelId.Id, e.Message.Author.Id));
                                break;
                            }
                        case "String":
                            {
                                if (!attr.HasParams)
                                {
                                    log.Warn($"Using parameter with type ({info.ParameterType.Name}) but flagged not using.");
                                }
                                parameters.Add(@params);
                                break;
                            }
                        default:
                            {
                                log.Error($"Unknown parameter with type ({info.ParameterType.Name}).");
                                return;
                            }
                    }
                }
                if (attr.NeedsVoice)
                {
                    DiscordGuildTextChannel channel = shard.Cache.GetGuildTextChannel(e.Message.ChannelId);
                    if (channel == null)
                    {
                        channel = await client.GetChannel<DiscordGuildTextChannel>(e.Message.ChannelId);
                        log.Debug($"Had to load guild data from API.(no cache)");
                    }
                    DiscordVoiceState voiceState = shard.Cache.GetVoiceState(channel.GuildId, e.Message.Author.Id);
                    DiscordVoiceConnection connection = shard.Voice.CreateOrGetConnection(channel.GuildId);
                    if (VoiceChannels.ContainsKey(channel.GuildId) && connection.IsValid && !connection.IsConnected && !connection.IsConnecting)
                    {
                        await connection.ConnectAsync(VoiceChannels[channel.GuildId].Id, startDeaf: true);
                        log.Debug($"Connected to voice channel in {Guilds.FirstOrDefault(g => g.Id == channel.GuildId)}");
                    }
                }
                Command.Value.Item2.Invoke(Command.Value.Item1, parameters.ToArray());
                st.Stop();
                log.Debug($"Command {Command.Key} took {st.Elapsed.ToString(@"s\.f")}s");
            }
            catch (Exception ex)
            {
                log.Info(ex);
            }
        }
    }
}
