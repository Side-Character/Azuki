using AzukiModuleApi;
using Discore;
using log4net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azuki.Core.CoreModules {
    internal class AdminModule : BaseModule {
        private readonly ILog log;
        public AdminModule() {
            log = LogManager.GetLogger("Azuki", "Core.Admin");
        }
        [Command(NeedsHandler = true, HasParams = true)]
        internal Task Broadcast(ICoreHandler handler, string broadcast) {
            log.Debug($"Broadcasting message ({broadcast}) ...");
            CoreHandler core = handler as CoreHandler;
            ICollection<DiscordGuildTextChannel> guildChannels = core.AdminChannels.Values;
            foreach (DiscordGuildTextChannel ch in guildChannels) {
                core.Respond(ch.Id, broadcast);
            }
            return Task.CompletedTask;
        }
        [Command()]
        internal Task ShutDown() {
            log.Debug($"Shutting Down.");
            AzukiCore.stopsignal.Set();
            return Task.CompletedTask;
        }
    }
}
