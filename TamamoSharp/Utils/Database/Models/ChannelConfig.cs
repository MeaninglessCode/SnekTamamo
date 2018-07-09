namespace TamamoSharp.Database
{
    public class ChannelConfig
    {
        public ulong ChannelId { get; set; }
        public bool IsIgnored { get; set; } = false;
        public ulong GuildId { get; set; }
    }
}
