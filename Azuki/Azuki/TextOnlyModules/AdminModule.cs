using Discore.Http;
using Discore.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chris.TextOnlyModules {
    class AdminModule {
        protected void AddHooks(IDiscordGateway gateway) { }
        #region Mute
        /*public void CreateMute(DiscordMessage Message) {
            DiscordGuildTextChannel channel = client.GetChannel(Message.ChannelId).Result as DiscordGuildTextChannel;
            Discore.Http.CreateGuildRoleOptions role = new Discore.Http.CreateGuildRoleOptions(1);
            role.SetHoisted(false);
            role.Name = "muted";
            role.SetPermissions(DiscordPermission.None);
            client.CreateGuildRole(channel.GuildId, role);
        }
        public void Mute(DiscordMessage Message, string name, string timer) {
            string[] split = name.Split("#");
            DiscordGuildTextChannel channel = client.GetChannel(Message.ChannelId).Result as DiscordGuildTextChannel;
            Snowflake id = 1;
            string Username = "";
            int Userid = 0;
            int time = Int32.Parse(timer);
            if (name[0] == '<' && name[1] == '@' && name[name.Length - 1] == '>' && name[name.Length - 5] != '#') {
                Tuple<Snowflake, string, int> at = At(name);
                id = at.Item1;
                Username = at.Item2;
                Userid = at.Item3;
            } else {
                Tuple<Snowflake, string, int> tuple = Name(Message, split);
                id = tuple.Item1;
                Username = tuple.Item2;
                Userid = tuple.Item3;
            }
            DiscordRole[] roles = client.GetGuildRoles(channel.GuildId).Result as DiscordRole[];
            bool exists = false;
            int count = 0;
            for (int i = 0; i < roles.Length; i++) {
                if (roles[i].Name == "muted") {
                    exists = true;
                    count = i;
                }
            }
            List<Discore.Http.OverwriteOptions> listoo = new List<Discore.Http.OverwriteOptions>();
            Discore.Http.OverwriteOptions OO = new Discore.Http.OverwriteOptions(roles[count].Id, DiscordOverwriteType.Role);
            DiscordPermission denied = (DiscordPermission)2112289271;
            DiscordPermission allowed = (DiscordPermission)34669568;
            OO.SetDeniedPermissions(denied);
            OO.SetAllowedPermissions(allowed);
            listoo.Add(OO);
            Discore.Http.GuildTextChannelOptions GO = new Discore.Http.GuildTextChannelOptions();
            Discore.Http.GuildVoiceChannelOptions GVO = new Discore.Http.GuildVoiceChannelOptions();
            GVO.SetPermissionOverwrites(listoo);
            GO.SetPermissionOverwrites(listoo);
            client.ModifyTextChannel(channel.Id, GO);
            DiscordGuild guild = client.GetGuild(channel.GuildId).Result;
            DiscordGuildChannel[] GC = guild.GetChannels().Result as DiscordGuildChannel[];
            for (int i = 0; i < GC.Length; i++) {
                if (GC[i].ChannelType == DiscordChannelType.GuildText) {
                    client.ModifyTextChannel(GC[i].Id, GO);
                } else {
                    client.ModifyVoiceChannel(GC[i].Id, GVO);
                }
            }
            DiscordRole[] dr = client.GetGuildRoles(channel.GuildId).Result as DiscordRole[];
            client.AddGuildMemberRole(channel.GuildId, id, roles[count].Id);
            System.Threading.Thread.Sleep(time * 1000);
            client.RemoveGuildMemberRole(channel.GuildId, id, roles[count].Id);
            /*Discore.Http.ModifyGuildMemberOptions option = new Discore.Http.ModifyGuildMemberOptions();
            option.SetServerMute(true);
            client.ModifyGuildMember(channel.GuildId, id, option);
            client.CreateMessage(Message.ChannelId, "Mute: " + "<@" + id + "> = " + option.IsServerMute);
            System.Threading.Thread.Sleep(time * 1000);
            option.SetServerMute(false);
            client.ModifyGuildMember(channel.GuildId, id, option);*/
        // }
        #endregion

        #region Kick
        /*public void Kick(DiscordMessage Message, string name) {
        string[] split = name.Split("#");
        DiscordGuildTextChannel channel = client.GetChannel(Message.ChannelId).Result as DiscordGuildTextChannel;
        string Username = "";
        int Userid = 0;
        Snowflake id = 1;
        if (name[0] == '<' && name[1] == '@' && name[name.Length - 1] == '>' && name[name.Length - 5] != '#') {
            Tuple<Snowflake, string, int> at = At(name);
            id = at.Item1;
            Username = at.Item2;
            Userid = at.Item3;
        } else {
            Tuple<Snowflake, string, int> tuple = Name(Message, split);
            id = tuple.Item1;
            Username = tuple.Item2;
            Userid = tuple.Item3;
        }
        client.RemoveGuildMember(channel.GuildId, id);
    }
    #endregion
    #region Ban
    public void Ban(DiscordMessage Message, string name, string time) {
        string[] split = name.Split("#");
        DiscordGuildTextChannel channel = client.GetChannel(Message.ChannelId).Result as DiscordGuildTextChannel;
        string Username = "";
        int Userid = 0;
        Snowflake id = 1;
        if (name[0] == '<' && name[1] == '@' && name[name.Length - 1] == '>' && name[name.Length - 5] != '#') {
            Tuple<Snowflake, string, int> at = At(name);
            id = at.Item1;
            Username = at.Item2;
            Userid = at.Item3;
        } else {
            Tuple<Snowflake, string, int> tuple = Name(Message, split);
            id = tuple.Item1;
            Username = tuple.Item2;
            Userid = tuple.Item3;
        }
        client.CreateGuildBan(channel.GuildId, id, 7);
    }*/
        #endregion
    }
}
