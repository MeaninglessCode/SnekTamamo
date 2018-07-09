namespace TamamoSharp.Database
{
    public class UserConfig
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public bool IsIgnored { get; set; } = false;
        public int CreatedTagCount { get; set; } = 0;
        public ulong GuildId { get; set; }
    }
}
