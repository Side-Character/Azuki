using Discore;
using Discore.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chris.TextOnlyModules {
    internal class RoleModule {
        /*protected void AddHooks(IDiscordGateway gateway) { }

        public async void Addrole(MessageEventArgs Message, string parameters) {
            bool done = false;
            string[] split = parameters.Split(" ", 2);
            string role = split[0];
            string name = split[1];
            DiscordGuildTextChannel channel = await client.GetChannel(Message.Message.ChannelId) as DiscordGuildTextChannel;
            DiscordGuild guild = await client.GetGuild(channel.GuildId);
            IReadOnlyList<DiscordRole> roles = await client.GetGuildRoles(channel.GuildId);
            DiscordRole selectedrole = roles.FirstOrDefault(r => r.Name == role);
            if (selectedrole == null) {
                client.CreateMessage(Message.Message.ChannelId, "Could not find role " + role);
                log.Info("Could not find role " + role);
                return;
                /*CreateRoleOptions options = new CreateRoleOptions(role);
                client.CreateGuildRole(guild.Id, options);*/
            /*}
            foreach (DiscordUser r in Message.Message.Mentions) {
                client.AddGuildMemberRole(guild.Id, r.Id, selectedrole.Id);
                client.CreateMessage(Message.Message.ChannelId, "Added role \"" + selectedrole.Name + "\" to <@" + r.Id + ">");
                log.Info("Added role \"" + selectedrole.Name + "\" to " + r.Username + "#" + r.Discriminator + " on server " + guild.Name);
                done = true;
            }
            if (!done)
            {
                foreach (DiscordGuildMember r in guild.ListGuildMembers().Result)
                {
                    if (r.User.Username == name)
                    {
                        client.AddGuildMemberRole(guild.Id, r.Id, selectedrole.Id);
                        client.CreateMessage(Message.Message.ChannelId, "Added role \"" + selectedrole.Name + "\" to <@" + r.Id + ">");
                        log.Info("Added role \"" + selectedrole.Name + "\" to " + r.User.Username + "#" + r.User.Discriminator + " on server " + guild.Name);
                        done = true;
                    }
                }
            }
        }

        public async void Remrole(MessageEventArgs Message, string parameters) {
            bool done = false;
            string[] split = parameters.Split(" ", 2);
            string role = split[0];
            string name = split[1];
            DiscordGuildTextChannel channel = await client.GetChannel(Message.Message.ChannelId) as DiscordGuildTextChannel;
            DiscordGuild guild = await client.GetGuild(channel.GuildId);
            IReadOnlyList<DiscordRole> roles = await client.GetGuildRoles(channel.GuildId);
            DiscordRole selectedrole = roles.FirstOrDefault(r => r.Name == role);
            if (selectedrole == null) {
                client.CreateMessage(Message.Message.ChannelId, "Could not find role \"" + role + "\"");
                log.Info("Could not find role \"" + role + "\"");
                return;
            }
            foreach (DiscordUser r in Message.Message.Mentions) {
                client.RemoveGuildMemberRole(guild.Id, r.Id, selectedrole.Id);
                client.CreateMessage(Message.Message.ChannelId, "Removed role \"" + selectedrole.Name + "\" from <@" + r.Id + ">");
                log.Info("Removed role \"" + selectedrole.Name + "\" from " + r.Username + "#" + r.Discriminator + " on server " + guild.Name);
                done = true;
            }
            if (!done)
            {
                foreach (DiscordGuildMember r in guild.ListGuildMembers().Result)
                {
                    if (r.User.Username == name)
                    {
                        client.RemoveGuildMemberRole(guild.Id, r.Id, selectedrole.Id);
                        client.CreateMessage(Message.Message.ChannelId, "Removed role \"" + selectedrole.Name + "\" to <@" + r.Id + ">");
                        log.Info("Removed role \"" + selectedrole.Name + "\" to " + r.User.Username + "#" + r.User.Discriminator + " on server " + guild.Name);
                        done = true;
                    }
                }
            }
        }

        public async void Colors(DiscordMessage Message, string colour) {
            colour = colour.Replace("#", string.Empty);
            string temp = colour[0].ToString() + colour[1].ToString();
            byte R = Convert.ToByte(temp, 16);
            temp = colour[2].ToString() + colour[3].ToString();
            byte G = Convert.ToByte(temp, 16);
            temp = colour[4].ToString() + colour[5].ToString();
            byte B = Convert.ToByte(temp, 16);
            DiscordGuildTextChannel channel = client.GetChannel(Message.ChannelId).Result as DiscordGuildTextChannel;
            DiscordGuild guild = client.GetGuild(channel.GuildId).Result;
            Discore.Http.CreateGuildRoleOptions role = new Discore.Http.CreateGuildRoleOptions(1);
            DiscordRole[] roles = client.GetGuildRoles(channel.GuildId).Result as DiscordRole[];
            //Item1 for Id, Item2 for name
            Tuple<Snowflake, string> roleid = Check(Message, R, G, B, roles);
            if (roleid.Item1 != 1) {
            } else {
                DiscordColor color = new DiscordColor(R, G, B) {
                    R = R,
                    G = G,
                    B = B
                };
                role.SetColor(color);
                role.SetHoisted(false);
                role.SetMentionable(false);
                role.SetPermissions(DiscordPermission.None);
                role.SetName("#" + R.ToString("X") + G.ToString("X") + B.ToString("X"));
                client.CreateGuildRole(channel.GuildId, role);
                roles = client.GetGuildRoles(channel.GuildId).Result as DiscordRole[];
                roleid = Check(Message, R, G, B, roles);
            }
            client.AddGuildMemberRole(channel.GuildId, Message.Author.Id, roleid.Item1);
            client.CreateMessage(Message.ChannelId, "Gave you the color: " + roleid.Item2);
        }
        private Tuple<Snowflake, string> Check(DiscordMessage Message, byte R, byte G, byte B, DiscordRole[] roles) {
            Tuple<Snowflake, string> tuple = new Tuple<Snowflake, string>(1, null);
            for (int i = 0; i < roles.Length; i++) {
                if (roles[i].Name == "#" + R.ToString("X") + G.ToString("X") + B.ToString("X")) {
                    tuple = new Tuple<Snowflake, string>(roles[i].Id, roles[i].Name);
                    return tuple;
                }
            }
            return tuple;
        }*/
    }
}
