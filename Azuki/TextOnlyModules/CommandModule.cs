using Chris.BaseClasses;
using Discore.WebSocket;
using System;
using System.Linq;
using Pubg;
using Pubg.Net;
using YoutubeExplode.Models.MediaStreams;
using System.Threading.Tasks;
using Chris.VoiceProviders;
using Discore;

namespace Chris.TextOnlyModules
{
    public class CommandModule
    {
        private YoutubeVoiceProvider voice = new YoutubeVoiceProvider();
        protected void AddHooks(IDiscordGateway gateway)
        {
            /*gateway.OnGuildMemberAdded += Gateway_OnGuildMemberAdded;
            gateway.OnGuildMemberRemoved += Gateway_OnGuildMemberRemoved;
            gateway.OnGuildCreated += Gateway_OnGuildCreated;*/
            //CoreBroadcastModule.Say("Are you ready to make some noise?");
        }

        private void Gateway_OnGuildCreated(object sender, GuildEventArgs e)
        {
            /*SharedObjectStorage.UpdateShardDependency();
            SharedObjectStorage*/
        }

        /*private void Gateway_OnGuildMemberAdded(object sender, GuildMemberEventArgs e)
        {
            client.CreateMessage(e.GuildId, "Welcome to the server <@" + e.Member.Id + ">");
        }

        private void Gateway_OnGuildMemberRemoved(object sender, GuildMemberEventArgs e)
        {
            client.CreateMessage(e.GuildId, "<@" + e.Member.Id + "> has left the guild. I'll miss them <3");
        }

        public void Help(MessageEventArgs e)
        {
            client.CreateMessage(e.Message.ChannelId, "This is the right way for starting :white_check_mark:\n!Help -> You are here right now\n!Join -> Join a voice channel\n!play (name) -> Play the specified music\n!asearch (name) -> Search for an anime\n!adownload (name) (episode) -> Get the download link of an anime");
        }

        public void Test(MessageEventArgs e, string part)
        {
            client.CreateMessage(e.Message.ChannelId, part);
        }*/
        /*public void test2(MessageEventArgs e, string part)
        {
            DiscordGuildTextChannel text = client.GetChannel(e.Message.ChannelId).Result as DiscordGuildTextChannel;
            var test = client.ListGuildEmojis(text.GuildId).Result;
            Snowflake[] directions = new Snowflake[16];
            string[] names = { "up", "down", "left", "right", "updown", "upleft", "upright", "downleft", "downright", "leftright", "updownleft", "updownright", "upleftright", "downleftright", "updownleftright" };
            for (int i = 0; i < test.Count; i++)
            {
                switch (test[i].Name)
                {
                    case "Up":
                        directions[0] = test[i].Id;
                        break;
                    case "Down":
                        directions[1] = test[i].Id;
                        break;
                    case "Left":
                        directions[2] = test[i].Id;
                        break;
                    case "Right":
                        directions[3] = test[i].Id;
                        break;
                    case "UpDown":
                        directions[4] = test[i].Id;
                        break;
                    case "UpLeft":
                        directions[5] = test[i].Id;
                        break;
                    case "UpRight":
                        directions[6] = test[i].Id;
                        break;
                    case "DownLeft":
                        directions[7] = test[i].Id;
                        break;
                    case "DownRight":
                        directions[8] = test[i].Id;
                        break;
                    case "LeftRight":
                        directions[9] = test[i].Id;
                        break;
                    case "UpDownLeft":
                        directions[10] = test[i].Id;
                        break;
                    case "UpDownRight":
                        directions[11] = test[i].Id;
                        break;
                    case "UpLeftRight":
                        directions[12] = test[i].Id;
                        break;
                    case "DownLeftRight":
                        directions[13] = test[i].Id;
                        break;
                    case "UpDownLeftRight":
                        directions[14] = test[i].Id;
                        break;
                }
            }
        }
        public void Pubg(MessageEventArgs e, string content)
        {
            string[] split = content.Split(" ");
            //string console = split[0];
            //string region = split[1];
            //string playername = split[2];
            var playerService = new PubgPlayerService("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJqdGkiOiI1Mzg5YTZlMC1mNjRmLTAxMzYtM2YwYS0xNzc1MzNlMGE0ZTQiLCJpc3MiOiJnYW1lbG9ja2VyIiwiaWF0IjoxNTQ3MDQ2OTMwLCJwdWIiOiJibHVlaG9sZSIsInRpdGxlIjoicHViZyIsImFwcCI6ImF6dWtpIn0.0gIWDH1w-UYIP2LY2x78ZDuHNiwK3Mu2AKkNq0YrGeU");
            var request = new GetPubgPlayersRequest
            {
                //ApiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJqdGkiOiI1Mzg5YTZlMC1mNjRmLTAxMzYtM2YwYS0xNzc1MzNlMGE0ZTQiLCJpc3MiOiJnYW1lbG9ja2VyIiwiaWF0IjoxNTQ3MDQ2OTMwLCJwdWIiOiJibHVlaG9sZSIsInRpdGxlIjoicHViZyIsImFwcCI6ImF6dWtpIn0.0gIWDH1w-UYIP2LY2x78ZDuHNiwK3Mu2AKkNq0YrGeU",
                PlayerNames = new string[] { content }
            };
            var players = playerService.GetPlayers(PubgPlatform.Steam, request);
            //PubgPlayer player = playerService.GetPlayer(PubgPlatform.Steam, players[0].Id, "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJqdGkiOiI1Mzg5YTZlMC1mNjRmLTAxMzYtM2YwYS0xNzc1MzNlMGE0ZTQiLCJpc3MiOiJnYW1lbG9ja2VyIiwiaWF0IjoxNTQ3MDQ2OTMwLCJwdWIiOiJibHVlaG9sZSIsInRpdGxlIjoicHViZyIsImFwcCI6ImF6dWtpIn0.0gIWDH1w-UYIP2LY2x78ZDuHNiwK3Mu2AKkNq0YrGeU");
            var matchService = new PubgMatchService("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJqdGkiOiI1Mzg5YTZlMC1mNjRmLTAxMzYtM2YwYS0xNzc1MzNlMGE0ZTQiLCJpc3MiOiJnYW1lbG9ja2VyIiwiaWF0IjoxNTQ3MDQ2OTMwLCJwdWIiOiJibHVlaG9sZSIsInRpdGxlIjoicHViZyIsImFwcCI6ImF6dWtpIn0.0gIWDH1w-UYIP2LY2x78ZDuHNiwK3Mu2AKkNq0YrGeU");
            var match = matchService.GetMatch(players.FirstOrDefault().MatchIds.First());
            var participants = match.Rosters.SelectMany(x => x.Participants);
            client.CreateMessage(e.Message.ChannelId, players.FirstOrDefault().MatchIds.First());
            client.CreateMessage(e.Message.ChannelId, match.CreatedAt);
            //client.CreateMessage(e.Message.ChannelId, participants.);
            int i = 0;
            int asked = 0;
            int winner = 0;
            while (i < participants.Count())
            {
                if (participants.ToArray()[i].Stats.Name == content)
                    asked = i;
                if (participants.ToArray()[i].Stats.WinPlace == 1)
                    winner = i;
                i++;
            }
            //Weapons Acquired: participants.ToArray()[i].Stats.WeaponsAcquired
            string output = "Name: " + participants.ToArray()[asked].Stats.Name + "\nDamage dealt: " + participants.ToArray()[asked].Stats.DamageDealt + "\nKills: " + participants.ToArray()[asked].Stats.Kills + "\nPlace: " + participants.ToArray()[asked].Stats.WinPlace + "\nNumber of downs: " + participants.ToArray()[asked].Stats.DBNOs;
            client.CreateMessage(e.Message.ChannelId, output);
            output = "Name: " + participants.ToArray()[winner].Stats.Name + "\nDamage dealt: " + participants.ToArray()[winner].Stats.DamageDealt + "\nKills: " + participants.ToArray()[winner].Stats.Kills + "\nPlace: " + participants.ToArray()[winner].Stats.WinPlace + "\nNumber of downs: " + participants.ToArray()[winner].Stats.DBNOs;
            client.CreateMessage(e.Message.ChannelId, output);

        }
        public void ylink(MessageEventArgs e, string content)
        {
            if (content.Contains("youtube.com"))
            {
                content = content.Split("?v=")[1];
            }
            client.CreateMessage(e.Message.ChannelId, voice.Link(content).Result);
        }
        public void createmap(MessageEventArgs e, string content)
        {
            string[] temp = content.Split(" "); DiscordGuildTextChannel text = client.GetChannel(e.Message.ChannelId).Result as DiscordGuildTextChannel;
            var test = client.ListGuildEmojis(text.GuildId).Result;
            Snowflake[] directions = new Snowflake[17];
            string[] names = { "up", "down", "left", "right", "updown", "upleft", "upright", "downleft", "downright", "leftright", "updownleft", "updownright", "upleftright", "downleftright", "updownleftright", "treasure" };
            for (int i = 0; i < test.Count; i++)
            {
                switch (test[i].Name)
                {
                    case "Up":
                        directions[0] = test[i].Id;
                        break;
                    case "Down":
                        directions[1] = test[i].Id;
                        break;
                    case "Left":
                        directions[2] = test[i].Id;
                        break;
                    case "Right":
                        directions[3] = test[i].Id;
                        break;
                    case "UpDown":
                        directions[4] = test[i].Id;
                        break;
                    case "UpLeft":
                        directions[5] = test[i].Id;
                        break;
                    case "UpRight":
                        directions[6] = test[i].Id;
                        break;
                    case "DownLeft":
                        directions[7] = test[i].Id;
                        break;
                    case "DownRight":
                        directions[8] = test[i].Id;
                        break;
                    case "LeftRight":
                        directions[9] = test[i].Id;
                        break;
                    case "UpDownLeft":
                        directions[10] = test[i].Id;
                        break;
                    case "UpDownRight":
                        directions[11] = test[i].Id;
                        break;
                    case "UpLeftRight":
                        directions[12] = test[i].Id;
                        break;
                    case "DownLeftRight":
                        directions[13] = test[i].Id;
                        break;
                    case "UpDownLeftRight":
                        directions[14] = test[i].Id;
                        break;
                    case "Treasure":
                        directions[15] = test[i].Id;
                        break;
                    case "Empty":
                        directions[16] = test[i].Id;
                        break;
                }
            }
            int x = Convert.ToInt32(temp[0]);
            int y = Convert.ToInt32(temp[1]);
            bool up = true;
            bool down = true;
            bool left = true;
            bool right = true;
            bool treasure = false;
            bool treasuredone = false;
            string[,] map = new string[x, y];
            string converter = "";
            string full = "";
            Random r = new Random();
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    int random = r.Next(0, 16);
                    if (random == 15 && !treasuredone)
                    {
                        treasuredone = true;
                        converter += "Treasure";
                        up = false;
                        down = false;
                        left = false;
                        right = false;
                    }
                    else
                    {
                        if (j == 0)
                        {
                            left = false;
                        }
                        else
                        if (j == x - 1)
                        {
                            right = false;
                        }
                        if (i == 0)
                        {
                            up = false;
                        }
                        else
                        if (i == y - 1)
                        {
                            down = false;
                        }
                    }
                    if (up)
                    {
                        int rando = r.Next(0, 4);
                        if (rando == 3)
                        {
                        }
                        else
                            converter += "Up";
                    }
                    if (down)
                    {
                        int rando = r.Next(0, 4);
                        if (rando == 3)
                        {
                        }
                        else
                            converter += "Down";
                    }
                    if (left)
                    {
                        int rando = r.Next(0, 4);
                        if (rando == 3)
                        {
                        }
                        else
                            converter += "Left";
                    }
                    if (right)
                    {
                        int rando = r.Next(0, 4);
                        if (rando == 3)
                        {
                        }
                        else
                            converter += "Right";
                    }
                    up = true;
                    down = true;
                    left = true;
                    right = true;
                    treasure = false;
                    if (i == y-1 && j == x-1 && !treasuredone)
                    {
                        full += "<:Treasure:" + directions[15] + ">";
                    }
                    else
                    {
                        if (converter == "")
                        {
                            full += "<:Empty:" + directions[16] + ">";
                        }
                        else
                        {
                            for (int a = 0; a < 16; a++)
                            {
                                if (converter.ToLower() == names[a])
                                {
                                    full += "<:" + names[a] + ":" + directions[a] + ">";
                                    Console.WriteLine(directions[a]);
                                }
                            }
                            converter = "";
                        }
                    }
                }
                full += "\n";
            }
            client.CreateMessage(e.Message.ChannelId, full);
            /*for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    int route = r.Next(0, 16);
                    if (route == 15)
                    {
                        treasure = true;
                    }
                    else
                    {
                        if (route % 4 == 0 && route != 0)
                        {
                            up = true;
                        }
                        if (route % 4 == 1 && route != 0)
                        {
                            down = true;
                        }
                        if (route % 4 == 2 && route != 0)
                        {
                            left = true;
                        }
                        if (route % 4 == 3 && route != 0)
                        {
                            up = true;
                        }
                        if (j == 0)
                        {
                            left = false;
                        }
                        else if (j == x)
                        {
                            right = false;
                        }
                        if (i == 0)
                        {
                            up = false;
                        }
                        else if (i == y)
                        {
                            down = false;
                        }
                    }
                    if (treasure)
                    {
                        full += "t";
                    }
                    if (up)
                    {
                        full += "u";
                    }
                    if (down)
                    {
                        full += "d";
                    }
                    if (left)
                    {
                        full += "l";
                    }
                    if (right)
                    {
                        full += "r";
                    }
                    full += "  ";
                    treasure = false;
                    up = false;
                    down = false;
                    left = false;
                    right = false;
                }
            full += "\n";
            }
            client.CreateMessage(e.Message.ChannelId, full);*/
        //}
        /*public void dm(MessageEventArgs e)
        {
            DiscordDMChannel dm = client.CreateDM(e.Message.Author.Id).Result;
            dm.CreateMessage("testing");
        }
        public void psay(MessageEventArgs e, string content)
        {
            //CoreBroadcastModule.Say(content);
        }
        public void run(MessageEventArgs e, string content) //RandomUserName
        {
            int length = Int32.Parse(content);
            Random r = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            client.CreateMessage(e.Message.ChannelId, new string(Enumerable.Repeat(chars, length)
              .Select(s => s[r.Next(s.Length)]).ToArray()));
        }*/
    }
}
