using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TamamoSharp.Database.GuildConfigs
{
    public class GuildConfig
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public bool IsIgnored { get; set; } = false;
        public bool NSFWEnabled { get; set; } = false;
        public List<IgnoredChannel> IgnoredChannels { get; set; }
        public List<IgnoredUser> IgnoredUsers { get; set; }
    }

    public class IgnoredChannel
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public GuildConfig GuildConfig { get; set; }
    }

    public class IgnoredUser
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public GuildConfig GuildConfig { get; set; }
    }
}
