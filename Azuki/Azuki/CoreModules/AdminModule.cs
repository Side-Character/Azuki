using AzukiModuleApi;
using Discore;
using log4net;
using System.Collections.Generic;

namespace Chris.AdminModules {
    internal class Admin : BaseModule {
        private readonly ILog log;

        public Admin() {
            log = LogManager.GetLogger("Azuki", "Core.Admin");
        }

        [Command(NeedsHandler = true, HasParams = true)]
        internal void Broadcast(ICoreHandler handler, string broadcast) {
            log.Debug($"Broadcasting message ({broadcast}) ...");
            CoreHandler core = handler as CoreHandler;
            ICollection<DiscordGuildTextChannel> guildChannels = core.AdminChannels.Values;
            foreach (DiscordGuildTextChannel ch in guildChannels) {
                core.Respond(ch.Id, broadcast);
            }
        }
        [Command()]
        internal void ShutDown() {
            log.Debug($"Shutting Down.");
            Chris.stopsignal.Set();
        }
    }
}
