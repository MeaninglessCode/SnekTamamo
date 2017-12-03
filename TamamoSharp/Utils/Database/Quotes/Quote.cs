using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TamamoSharp.Database.Quotes
{
    public class Quote
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public ulong OwnerId { get; set; }
        public ulong GuildId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int Uses { get; set; } = 0;
    }
}
