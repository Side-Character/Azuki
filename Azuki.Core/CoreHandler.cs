using Azuki.Core.Properties;
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

namespace Azuki.Core {
    internal class CoreHandler : ICoreHandler {
        private readonly ILog log;
        private readonly DiscordHttpClient client;
        private readonly Shard shard;
        private readonly Dictionary<string, Tuple<BaseModule, MethodInfo>> commands;
        internal List<DiscordGuild> Guilds { get; } = new List<DiscordGuild>();
        internal Dictionary<Snowflake, DiscordGuildTextChannel> AdminChannels { get; } = new Dictionary<Snowflake, DiscordGuildTextChannel>();
        internal Dictionary<Snowflake, DiscordGuildVoiceChannel> VoiceChannels { get; } = new Dictionary<Snowflake, DiscordGuildVoiceChannel>();
        public CoreHandler(DiscordHttpClient client, Shard shard, Dictionary<string, Tuple<BaseModule, MethodInfo>> commands) {
            log = LogManager.GetLogger("Azuki", "Core.Handler");
            this.client = client;
            this.shard = shard;
            this.commands = commands;
            shard.Gateway.OnGuildCreated += DiscoveredGuild;
            shard.Gateway.OnGuildAvailable += DiscoveredGuild;
            shard.Gateway.OnMessageCreated += Gateway_OnMessageCreated;
        }
        private void DiscoveredGuild(object sender, GuildEventArgs e) {
            log.Debug($"Discovered guild ({e.Guild.Name})");
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
                    List<KeyValuePair<string, Tuple<BaseModule, MethodInfo>>> possibleCommands = commands.Where(t => t.Key.Substring(t.Key.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase) + 1).Equals(command, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    if (AzukiCore.Admins.Contains(e.Message.Author.Id)) {
                        possibleCommands.AddRange(AzukiCore.AdminCommands.Where(t => t.Key.Substring(t.Key.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase) + 1).Equals(command, StringComparison.CurrentCultureIgnoreCase)).ToList());
                    }
                    if (possibleCommands.Count <= 0) {
                        _ = client.CreateMessage(e.Message.ChannelId, $"404 - Command not found. ({command})");
                        return;
                    } else if (possibleCommands.Count > 1) {
                        _ = client.CreateMessage(e.Message.ChannelId, "Multiple Commands found.");
                        log.Warn($"Multiple Commands found. ({string.Join(',', possibleCommands)})");
                        return;
                    }
                    ExecuteCommand(possibleCommands.FirstOrDefault(), e, paramstring);
                }
            } catch (Exception ex) {
                log.Error(ex.ToString());
                throw;
            }
        }

        private async void ExecuteCommand(KeyValuePair<string, Tuple<BaseModule, MethodInfo>> Command, MessageEventArgs e, string @params) {
            try {
                log.Debug("Executing command: " + Command.Key + ":" + ((string.IsNullOrEmpty(@params)) ? "no params" : @params));
                Stopwatch st = new Stopwatch();
                st.Start();
                MethodInfo method = Command.Value.Item2;
                CommandAttribute attr = method.GetCustomAttribute<CommandAttribute>();
                List<object> parameters = new List<object>();
                foreach (ParameterInfo info in method.GetParameters()) {
                    Type t = info.ParameterType;
                    if (parameters.Exists(p => p.GetType().Equals(t))) {
                        log.Error($"Duplicate parameter with type ({info.ParameterType.Name}).");
                        return;
                    }
                    switch (t.Name) {
                        case "ICoreHandler": {
                                if (!attr.NeedsHandler) {
                                    log.Warn($"Using parameter with type ({info.ParameterType.Name}) but flagged not using.");
                                }
                                parameters.Add(this);
                                break;
                            }
                        case "Message": {
                                if (!attr.NeedsMessage) {
                                    log.Warn($"Using parameter with type ({info.ParameterType.Name}) but flagged not using.");
                                }
                                parameters.Add(new Message(e.Message.Id, e.Message.ChannelId.Id, e.Message.Author.Id));
                                break;
                            }
                        case "String": {
                                if (!attr.HasParams) {
                                    log.Warn($"Using parameter with type ({info.ParameterType.Name}) but flagged not using.");
                                }
                                parameters.Add(@params);
                                break;
                            }
                        default: {
                                log.Error($"Unknown parameter with type ({info.ParameterType.Name}).");
                                return;
                            }
                    }
                }
                if (attr.NeedsVoice) {
                    DiscordGuildTextChannel channel = shard.Cache.GetGuildTextChannel(e.Message.ChannelId);
                    if (channel == null) {
                        channel = await client.GetChannel<DiscordGuildTextChannel>(e.Message.ChannelId).ConfigureAwait(true);
                        log.Debug("Had to load guild data from API.(no cache)");
                    }
                    DiscordVoiceState voiceState = shard.Cache.GetVoiceState(channel.GuildId, e.Message.Author.Id);
                    DiscordVoiceConnection connection = shard.Voice.CreateOrGetConnection(channel.GuildId);
                    if (VoiceChannels.ContainsKey(channel.GuildId) && connection.IsValid && !connection.IsConnected && !connection.IsConnecting) {
                        await connection.ConnectAsync(VoiceChannels[channel.GuildId].Id, startDeaf: true).ConfigureAwait(true);
                        log.Debug($"Connected to voice channel in {Guilds.FirstOrDefault(g => g.Id == channel.GuildId)}");
                    }
                }
                await Task.Run(() => {
                    try {
                        Command.Value.Item2.Invoke(Command.Value.Item1, parameters.ToArray());
                    } catch (Exception ex) {
                        log.Error(ex);
                        throw;
                    }
                }).ConfigureAwait(true);
                st.Stop();
                log.Debug($"Command {Command.Key} took {st.Elapsed:s\\.f}s");
            } catch (Exception ex) {
                log.Error(ex);
                throw;
            }
        }
    }
}
