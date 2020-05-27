namespace Azuki.Core.Modules.Api {
    public interface ICoreHandler {
        void Respond(ulong channelid, string message);
        void React(ulong channelid, ulong messageid, string emojiname);
        void EditMessage(ulong channelid, ulong messageid, string content);
    }
}
