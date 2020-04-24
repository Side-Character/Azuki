using System;
using System.Collections.Generic;
using System.Text;

namespace AzukiModuleApi {
    public interface ICoreHandler {
        void Respond(ulong channelid, string message);
        void React(ulong channelid, ulong messageid, string emojiname);
        void EditMessage(ulong channelid, ulong messageid, string content);
    }
}
