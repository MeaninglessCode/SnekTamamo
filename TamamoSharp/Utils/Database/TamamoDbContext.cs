using Microsoft.EntityFrameworkCore;

namespace TamamoSharp.Database
{
    public class TamamoDbContext : DbContext
    {
        public DbSet<GuildConfig> GuildConfigs { get; private set; }
        public DbSet<ChannelConfig> ChannelConfigs { get; private set; }
        public DbSet<UserConfig> UserConfigs { get; private set; }
        public DbSet<Quote> Quotes { get; private set; }
        public DbSet<Tag> Tags { get; private set; }
        public DbSet<TagAlias> TagAliases { get; private set; }

        public TamamoDbContext(DbContextOptions options): base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Guild DbSets
            builder.Entity<GuildConfig>()
                .HasKey(x => x.GuildId);
            builder.Entity<GuildConfig>()
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<GuildConfig>()
                .Property(x => x.IsIgnored)
                .IsRequired();

            builder.Entity<ChannelConfig>()
                .HasKey(x => x.ChannelId);
            builder.Entity<ChannelConfig>()
                .Property(x => x.ChannelId)
                .IsRequired();
            builder.Entity<ChannelConfig>()
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<ChannelConfig>()
                .HasOne(x => x.GuildConfig)
                .WithMany(x => x.IgnoredChannels)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserConfig>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Entity<UserConfig>()
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<UserConfig>()
                .Property(x => x.UserId)
                .IsRequired();
            builder.Entity<UserConfig>()
                .HasOne(x => x.GuildConfig)
                .WithMany(x => x.IgnoredUsers)
                .OnDelete(DeleteBehavior.Cascade);

            // Quote DbSets
            builder.Entity<Quote>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.Name)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.Content)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.OwnerId)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.CreatedAt)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.UpdatedAt)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.Uses)
                .IsRequired();

            // Tag DbSets
            builder.Entity<Tag>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Entity<Tag>()
                .Property(x => x.Name)
                .IsRequired();
            builder.Entity<Tag>()
                .Property(x => x.Content)
                .IsRequired();
            builder.Entity<Tag>()
                .Property(x => x.OwnerId)
                .IsRequired();
            builder.Entity<Tag>()
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<Tag>()
                .Property(x => x.Type)
                .IsRequired();
            builder.Entity<Tag>()
                .Property(x => x.CreatedAt)
                .IsRequired();
            builder.Entity<Tag>()
                .Property(x => x.UpdatedAt)
                .IsRequired();
            builder.Entity<Tag>()
                .Property(x => x.Uses)
                .IsRequired();

            builder.Entity<TagAlias>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Entity<TagAlias>()
                .Property(x => x.TagId)
                .IsRequired();
            builder.Entity<TagAlias>()
                .Property(x => x.Name)
                .IsRequired();
            builder.Entity<TagAlias>()
                .HasOne(x => x.Tag)
                .WithMany(x => x.Aliases)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
