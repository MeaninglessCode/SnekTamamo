using System.Collections.Generic;

namespace TamamoSharp.Database
{
    public class GuildIgnoreData
    {
        public ulong GuildId { get; set; }
        public bool IsGuildIgnored { get; set; }
        public Dictionary<ulong, bool> ChannelData { get; set; }
    }
}
