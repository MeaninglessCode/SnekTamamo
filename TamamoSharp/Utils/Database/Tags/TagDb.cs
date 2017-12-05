using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TamamoSharp.Database.Tags
{
    public class TagDb : DbContext
    {
        public DbSet<Tag> Tags { get; private set; }
        public DbSet<TagAlias> Aliases { get; private set; }

        public TagDb() { Database.EnsureCreated(); }

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
                .Property(x => x.OwnerId)
                .IsRequired();
            builder.Entity<TagAlias>()
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<TagAlias>()
                .HasOne(x => x.Tag)
                .WithMany(x => x.Aliases)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public async Task AddTagAsync(Tag t)
        {
            await Tags.AddAsync(t);
            await SaveChangesAsync();
        }

        public async Task DeleteTagAsync(Tag t)
        {
            Remove(t);
            await SaveChangesAsync();
        }

        public async Task<Tag> GetTagAsync(ulong guildId, string name)
            => await Tags.SingleOrDefaultAsync(x => x.GuildId == guildId &&
               ((x.Name == name) || (x.Aliases.Any(y => y.Name == name))));

        public async Task<Tag[]> GetTagsAsync(ulong guildId)
            => await Tags.Where(x => x.GuildId == guildId).ToArrayAsync();

        public async Task AddAliasAsync(TagAlias a)
        {
            await Aliases.AddAsync(a);
            await SaveChangesAsync();
        }

        public async Task DeleteAliasAsync(TagAlias a)
        {
            Remove(a);
            await SaveChangesAsync();
        }

        public async Task<TagAlias> GetAliasAsync(ulong guildId, string name)
            => await Aliases.Include(x => x.Tag).SingleOrDefaultAsync(x =>
               x.GuildId == guildId && x.Name.ToLower() == name.ToLower());

        public async Task<TagAlias[]> GetAliasesAsync(int tagId)
            => await Aliases.Where(x => x.TagId == tagId).ToArrayAsync();
    }
}
