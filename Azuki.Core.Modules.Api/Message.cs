using System;

namespace Azuki.Core.Modules.Api {
    public class Message {
        public ulong Id { get; private set; }
        public ulong ChannelId { get; private set; }
        public ulong AuthorId { get; private set; }
        public ulong VoiceId { get; private set; }
        public event EventHandler<byte[]> HasSound;
        public Message(ulong id, ulong channelid, ulong authorid, ulong voiceId = 0) {
            Id = id;
            ChannelId = channelid;
            AuthorId = authorid;
            VoiceId = voiceId;
        }
        public void TransferSound(byte[] data) {
            HasSound?.Invoke(this, data);
        }
    }
}
