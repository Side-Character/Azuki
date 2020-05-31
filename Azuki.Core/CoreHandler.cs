using Azuki.Core.Modules.Api;
using Azuki.Core.Properties;
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

namespace Azuki.Core {
    internal class CoreHandler : ICoreHandler {
        private readonly ILog log;
        private readonly DiscordHttpClient client;
        private readonly Shard shard;
        private readonly Dictionary<string, Tuple<BaseModule, ILog, MethodInfo>> commands;
        internal List<DiscordGuild> Guilds { get; } = new List<DiscordGuild>();
        internal Dictionary<Snowflake, DiscordGuildVoiceChannel> VoiceChannels { get; } = new Dictionary<Snowflake, DiscordGuildVoiceChannel>();
        public CoreHandler(DiscordHttpClient client, Shard shard, Dictionary<string, Tuple<BaseModule, ILog, MethodInfo>> commands) {
            log = LogManager.GetLogger("Azuki", "Core.Handler");
            this.client = client;
            this.shard = shard;
            this.commands = commands;
            shard.Gateway.OnGuildCreated += DiscoveredGuild;
            shard.Gateway.OnGuildAvailable += DiscoveredGuild;
            shard.Gateway.OnMessageCreated += Gateway_OnMessageCreated;
        }
        private void DiscoveredGuild(object sender, GuildEventArgs e) {
            log.Debug(string.Format(Resources.Culture, Resources.ResourceManager.GetString("DiscoveredGuild", Resources.Culture), e.Guild.Name));
            Guilds.Add(e.Guild);
        }
        internal void Respond(Snowflake channel, string message) {
            client.CreateMessage(channel, message);
        }
        public void Respond(ulong channelid, string message) {
            client.CreateMessage(new Snowflake(channelid), message);
        }
        internal void React(Snowflake channel, Snowflake message, string emojiname) {
            client.CreateReaction(channel, message, new DiscordReactionEmoji(emojiname));
        }
        public void React(ulong channelid, ulong messageid, string emojiname) {
            client.CreateReaction(new Snowflake(channelid), new Snowflake(messageid), new DiscordReactionEmoji(emojiname));
        }
        internal void EditMessage(Snowflake channel, Snowflake message, string content) {
            client.EditMessage(channel, message, content);
        }
        public void EditMessage(ulong channelid, ulong messageid, string content) {
            client.EditMessage(new Snowflake(channelid), new Snowflake(messageid), content);
        }
        protected virtual void Gateway_OnMessageCreated(object sender, MessageEventArgs e) {
            try {
                if (!e.Message.Author.IsBot) {
                    if (!e.Message.Content.StartsWith(AzukiCore.config.DefaultStartChar)) {
                        return;
                    }
                    List<string> split = e.Message.Content.Split(" ", 2).ToList();
                    string command = split.FirstOrDefault().TrimStart(AzukiCore.config.DefaultStartChar).ToLower(Resources.Culture);
                    string paramstring = "";
                    if (split.Count > 1) {
                        paramstring = split[1];
                    }
                    List<KeyValuePair<string, Tuple<BaseModule, ILog, MethodInfo>>> possibleCommands = commands.Where(t => t.Key.Substring(t.Key.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase) + 1).Equals(command, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    if (AzukiCore.Admins.Contains(e.Message.Author.Id)) {
                        possibleCommands.AddRange(AzukiCore.AdminCommands.Where(t => t.Key.Substring(t.Key.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase) + 1).Equals(command, StringComparison.CurrentCultureIgnoreCase)).ToList());
                    }
                    if (possibleCommands.Count <= 0) {
                        _ = client.CreateMessage(e.Message.ChannelId, string.Format(Resources.Culture, Resources.ResourceManager.GetString("CommandNotFound", Resources.Culture), command));
                        return;
                    } else if (possibleCommands.Count > 1) {
                        _ = client.CreateMessage(e.Message.ChannelId, string.Format(Resources.Culture, Resources.ResourceManager.GetString("MultipleCommandsFound", Resources.Culture), string.Join(',', possibleCommands)));
                        return;
                    }
                    ExecuteCommand(possibleCommands.FirstOrDefault(), e, paramstring);
                }
            } catch (Exception ex) {
                log.Error(ex.ToString());
                throw;
            }
        }

        private async void ExecuteCommand(KeyValuePair<string, Tuple<BaseModule, ILog, MethodInfo>> Command, MessageEventArgs e, string @params) {
            try {
                log.Debug(string.Format(Resources.Culture, Resources.ResourceManager.GetString("ExecutingCommand", Resources.Culture), Command.Key, string.IsNullOrEmpty(@params) ? Resources.ResourceManager.GetString("NoParams", Resources.Culture) : @params));
                Stopwatch st = new Stopwatch();
                st.Start();
                MethodInfo method = Command.Value.Item3;
                CommandAttribute attr = method.GetCustomAttribute<CommandAttribute>();
                List<object> parameters = new List<object>();
                foreach (ParameterInfo info in method.GetParameters()) {
                    Type t = info.ParameterType;
                    if (parameters.Exists(p => p.GetType().Equals(t))) {
                        Command.Value.Item2.Error(string.Format(Resources.Culture, Resources.ResourceManager.GetString("UsingDuplicateParam", Resources.Culture), info.Name, info.ParameterType.Name));
                        return;
                    }
                    DiscordVoiceConnection connection = null;
                    if (attr.NeedsVoice) {
                        DiscordGuildTextChannel channel = shard.Cache.GetGuildTextChannel(e.Message.ChannelId);
                        if (channel == null) {
                            channel = await client.GetChannel<DiscordGuildTextChannel>(e.Message.ChannelId).ConfigureAwait(true);
                            log.Debug(Resources.ResourceManager.GetString("HadToLoadGuild", Resources.Culture));
                        }
                        DiscordVoiceState voiceState = shard.Cache.GetVoiceState(channel.GuildId, e.Message.Author.Id);
                        if (voiceState.ChannelId.HasValue) {
                            connection = shard.Voice.CreateOrGetConnection(channel.GuildId);
                            if (connection.IsValid && !connection.IsConnected && !connection.IsConnecting) {
                                await connection.ConnectAsync(voiceState.ChannelId.Value, startDeaf: true).ConfigureAwait(true);
                                log.Debug(string.Format(Resources.Culture, Resources.ResourceManager.GetString("ConnectedToVoiceChannel", Resources.Culture), voiceState.ChannelId.Value, Guilds.FirstOrDefault(g => g.Id == channel.GuildId)));
                            }
                        } else {
                            return;
                        }
                    }
                    switch (t.Name) {
                        case "ICoreHandler": {
                                if (!attr.NeedsHandler) {
                                    Command.Value.Item2.Warn(string.Format(Resources.Culture, Resources.ResourceManager.GetString("UsingUnflaggedParam", Resources.Culture), info.Name, info.ParameterType.Name));
                                }
                                parameters.Add(this);
                                break;
                            }
                        case "Message": {
                                if (!attr.NeedsMessage) {
                                    Command.Value.Item2.Warn(string.Format(Resources.Culture, Resources.ResourceManager.GetString("UsingUnflaggedParam", Resources.Culture), info.Name, info.ParameterType.Name));
                                }
                                Message message = new Message(e.Message.Id, e.Message.ChannelId.Id, e.Message.Author.Id);
                                if (attr.NeedsVoice) {
                                    message.HasSound += Message_HasSound;
                                }
                                parameters.Add(message);
                                break;
                            }
                        case "String": {
                                if (!attr.HasParams) {
                                    Command.Value.Item2.Warn(string.Format(Resources.Culture, Resources.ResourceManager.GetString("UsingUnflaggedParam", Resources.Culture), info.Name, info.ParameterType.Name));
                                }
                                parameters.Add(@params);
                                break;
                            }
                        default: {
                                Command.Value.Item2.Error(string.Format(Resources.Culture, Resources.ResourceManager.GetString("UsingUnknownParam", Resources.Culture), info.Name, info.ParameterType.Name));
                                return;
                            }
                    }
                }
                await Task.Run(() => {
                    try {
                        if (Command.Value.Item3.IsStatic) {
                            Command.Value.Item3.Invoke(null, parameters.ToArray());
                        } else {
                            Command.Value.Item3.Invoke(Command.Value.Item1, parameters.ToArray());
                        }
                    } catch (InvalidOperationException ex) {
                        Command.Value.Item2.Error(ex.InnerException.ToString());
                    }
                }).ConfigureAwait(true);
                st.Stop();
                log.Debug(string.Format(Resources.Culture, Resources.ResourceManager.GetString("RanCommand", Resources.Culture), Command.Key, st.Elapsed.ToString("s\\.f", Resources.Culture)));
            } catch (Exception ex) {
                log.Error(ex);
                throw;
            }
        }

        private void Message_HasSound(object sender, byte[] e) {
            Message message = sender as Message;
            if (message != null) {

            }
        }

        public override string ToString() {
            return string.Format(Resources.Culture, Resources.ResourceManager.GetString("HandlerStatus", Resources.Culture), shard.Id, shard.IsRunning);
        }
    }
}
