using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TamamoSharp.Database.GuildConfigs
{
    public class GuildConfigDb : DbContext
    {
        public DbSet<GuildConfig> GuildConfigs { get; private set; }
        public DbSet<IgnoredChannel> IgnoredChannels { get; private set; }
        public DbSet<IgnoredUser> IgnoredUsers { get; private set; }

        public GuildConfigDb() { Database.EnsureCreated(); }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "data");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string dataDir = Path.Combine(dir, "guild_configs.db");
            builder.UseSqlite($"Filename={dataDir}");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GuildConfig>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Entity<GuildConfig>()
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<GuildConfig>()
                .Property(x => x.NSFWEnabled)
                .IsRequired();

            builder.Entity<IgnoredChannel>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Entity<IgnoredChannel>()
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<IgnoredChannel>()
                .Property(x => x.ChannelId)
                .IsRequired();
            builder.Entity<IgnoredChannel>()
                .HasOne(x => x.GuildConfig)
                .WithMany(x => x.IgnoredChannels)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<IgnoredUser>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Entity<IgnoredUser>()
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<IgnoredUser>()
                .Property(x => x.UserId)
                .IsRequired();
            builder.Entity<IgnoredUser>()
                .HasOne(x => x.GuildConfig)
                .WithMany(x => x.IgnoredUsers)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
