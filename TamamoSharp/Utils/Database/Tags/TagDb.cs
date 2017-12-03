using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TamamoSharp.Database.Tags
{
    class TagDb : DbContext
    {
        public DbSet<Tag> Tags { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "data");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string dataDir = Path.Combine(dir, "tags.db");
            builder.UseSqlite($"Filename={dataDir}");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

        }
    }
}
