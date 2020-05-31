using Azuki.Core.Modules.Api;
using Discore;
using log4net;
using System;
using System.Threading.Tasks;

namespace Azuki.Core.CoreModules {
#pragma warning disable CA1812
    internal class AdminModule : BaseModule {
        private readonly ILog log;
        public AdminModule() {
            log = LogManager.GetLogger("Azuki", "Core.CoreModules.Admin");
        }
        [Command(NeedsHandler = true, NeedsMessage = true)]
        internal static Task Status(ICoreHandler handler, Message Message) {
            CoreHandler core = handler as CoreHandler;
            core.Respond(Message.ChannelId, core.ToString());
            return Task.CompletedTask;
        }
        [Command(NeedsHandler = true, HasParams = true)]
        internal Task Broadcast(ICoreHandler handler, string broadcast) {
            try {
                CoreHandler core = handler as CoreHandler;
                foreach (DiscordGuild guild in core.Guilds) {
                    if (guild.SystemChannelId.HasValue) {
                        core.Respond(guild.SystemChannelId.Value, broadcast);
                    } else {
                        core.Respond(guild.Id, broadcast);
                    }
                }
            } catch (Exception ex) {
                log.Error(ex.ToString());
                throw;
            }
            return Task.CompletedTask;
        }
        [Command()]
        internal static Task ShutDown() {
            _ = AzukiCore.stopsignal.Set();
            return Task.CompletedTask;
        }
    }
#pragma warning restore CA1812
}
