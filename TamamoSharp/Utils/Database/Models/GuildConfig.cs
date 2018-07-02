using System.Collections.Generic;

namespace TamamoSharp.Database
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public bool IsIgnored { get; set; } = false;
        public bool LeaveMessageEnabled { get; set; } = false;
        public string LeaveMessage { get; set; } = "%user% left the guild. :(";
        public bool JoinMessageEnabled { get; set; } = false;
        public string JoinMessage { get; set; } = "Welcome, %user%!";
        public List<ChannelConfig> IgnoredChannels { get; set; }
        public List<UserConfig> IgnoredUsers { get; set; }
    }

    public class ChannelConfig
    {
        public ulong ChannelId { get; set; }
        public bool IsIgnored { get; set; } = false;
        public ulong GuildId { get; set; }
        public GuildConfig GuildConfig { get; set; }
    }

    public class UserConfig
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public GuildConfig GuildConfig { get; set; }
    }
}
