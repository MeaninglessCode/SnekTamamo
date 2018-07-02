using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TamamoSharp.Database
{
    public static class TagExtensions
    {
        public static async Task AddTagAsync(this TamamoDbContext ctx, Tag tag)
        {
            await ctx.Tags.AddAsync(tag);
            await ctx.SaveChangesAsync();
        }

        public static async Task AddTagAliasAsync(this TamamoDbContext ctx, TagAlias alias)
        {
            await ctx.TagAliases.AddAsync(alias);
            await ctx.SaveChangesAsync();
        }

        public static async Task DeleteTagAsync(this TamamoDbContext ctx, Tag tag)
        {
            ctx.Remove(tag);
            await ctx.SaveChangesAsync();
        }

        public static async Task DeleteTagAliasAsync(this TamamoDbContext ctx, TagAlias alias)
        {
            ctx.Remove(alias);
            await ctx.SaveChangesAsync();
        }

        public static async Task<Tag> GetTagAsync(this TamamoDbContext ctx, ulong guildId, string name)
            => await ctx.Tags.FirstOrDefaultAsync(t => t.GuildId == guildId &&
                (t.Name == name || t.Aliases.Any(e => e.Name == name)));

        public static async Task<Tag[]> GetAllTagsAsync(this TamamoDbContext ctx, ulong guildId)
            => await ctx.Tags.Where(t => t.GuildId == guildId).ToArrayAsync();

        public static async Task<TagAlias> GetTagAliasAsync(this TamamoDbContext ctx, ulong guildId, string name)
            => await ctx.TagAliases.FirstOrDefaultAsync(a => a.Name == name);


        public static async Task<TagAlias[]> GetAllTagAliasesAsync(this TamamoDbContext ctx, int tagId)
            => await ctx.TagAliases.Where(t => t.TagId == tagId).ToArrayAsync();

        public static async Task AddTagUseAsync(this TamamoDbContext ctx, Tag tag)
        {
            tag.Uses += 1;
            ctx.Update(tag);
            await ctx.SaveChangesAsync();
        }
    }
}
