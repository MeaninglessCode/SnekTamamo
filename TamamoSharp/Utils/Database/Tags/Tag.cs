using System;
using System.Collections.Generic;

namespace TamamoSharp.Database.Tags
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public ulong OwnerId { get; set; }
        public ulong GuildId { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int Uses { get; set; } = 0;
        public List<TagAlias> Aliases { get; set; }
    }

    public class TagAlias
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public string Name { get; set; }
        public ulong OwnerId { get; set; }
        public ulong GuildId { get; set; }
        public Tag Tag { get; set; }
    }
}
