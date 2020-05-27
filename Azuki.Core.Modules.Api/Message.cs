namespace Azuki.Core.Modules.Api {
    public class Message {
        public ulong Id { get; private set; }
        public ulong ChannelId { get; private set; }
        public ulong AuthorId { get; private set; }
        public Message(ulong id, ulong channelid, ulong authorid) {
            Id = id;
            ChannelId = channelid;
            AuthorId = authorid;
        }
    }
}
