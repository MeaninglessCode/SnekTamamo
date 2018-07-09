using System;

namespace TamamoSharp.Database
{
    public class StarboardEntry
    {
        public Guid Id { get; set; }
        public ulong MessageId { get; set; }
        public ulong BotMessageId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong StarboardChannelId { get; set; }
        public ulong AuthorId { get; set; }
        public int StarCount { get; set; }
        public ulong GuildId { get; set; }
    }
}
