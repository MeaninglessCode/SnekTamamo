using System;
using System.Collections.Generic;

namespace TamamoSharp.Database
{
    public class Tag
    {
        public TagData Data { get; set; }
        public List<TagAlias> Aliases { get; set; }
    }

    public class TagData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public ulong OwnerId { get; set; }
        public ulong GuildId { get; set; }
        public string Type { get; set; } = "guild";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int Uses { get; set; } = 0;
    }

    public class TagAlias
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ulong OwnerId { get; set; }
        public Guid TagId { get; set; }
    }
}
