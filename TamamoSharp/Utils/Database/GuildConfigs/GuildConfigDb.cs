using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<GuildConfig>()
                .Property(x => x.IsIgnored)
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

        public async Task AddGuildAsync(GuildConfig gc)
        {
            await GuildConfigs.AddAsync(gc);
            await SaveChangesAsync();
        }

        public async Task<bool> GuildIgnoredAsync(ulong guildId)
        {
            GuildConfig c = await GetGuildConfigAsync(guildId);
            if (c == null)
                return false;
            else
                return c.IsIgnored;
        }

        public async Task IgnoreGuildAsync(ulong guildId)
        {
            GuildConfig c = await GetGuildConfigAsync(guildId);
            c.IsIgnored = true;
            Update(c);
            await SaveChangesAsync();
        }

        public async Task UnIgnoreGuildAsync(ulong guildId)
        {
            GuildConfig c = await GetGuildConfigAsync(guildId);
            c.IsIgnored = false;
            Update(c);
            await SaveChangesAsync();
        }

        public async Task<GuildConfig> GetGuildConfigAsync(ulong guildId)
            => await GuildConfigs.SingleOrDefaultAsync(x => x.GuildId == guildId);

        public async Task IgnoreUserAsync(IgnoredUser u)
        {
            await IgnoredUsers.AddAsync(u);
            await SaveChangesAsync();
        }

        public async Task UnIgnoreUserAsync(IgnoredUser u)
        {
            Remove(u);
            await SaveChangesAsync();
        }

        public async Task<bool> UserIgnoredAsync(ulong guildId, ulong userId)
            => (await GetIgnoredUserAsync(guildId, userId) == null)? false : true;

        public async Task<IgnoredUser> GetIgnoredUserAsync(ulong guildId, ulong userId)
            => await IgnoredUsers.Include(x => x.GuildConfig).SingleOrDefaultAsync(x =>
               x.GuildId == guildId && x.UserId == userId);

        public async Task IgnoreChannelAsync(IgnoredChannel c)
        {
            await IgnoredChannels.AddAsync(c);
            await SaveChangesAsync();
        }

        public async Task UnIgnoreChannelAsync(IgnoredChannel c)
        {
            Remove(c);
            await SaveChangesAsync();
        }

        public async Task<bool> ChannelIgnoredAsync(ulong guildId, ulong channelId)
            => (await GetIgnoredChannelAsync(guildId, channelId) == null) ? false : true;

        public async Task<IgnoredChannel> GetIgnoredChannelAsync(ulong guildId, ulong channelId)
            => await IgnoredChannels.Include(x => x.GuildConfig).SingleOrDefaultAsync(x =>
               x.GuildId == guildId && x.ChannelId == channelId);
    }
}
