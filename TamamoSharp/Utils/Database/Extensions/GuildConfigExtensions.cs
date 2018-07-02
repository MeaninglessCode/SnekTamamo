using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TamamoSharp.Database
{
    public static class GuildConfigExtensions
    {
        public static async Task AddGuildConfigAsync(this TamamoDbContext ctx, GuildConfig config)
        {
            await ctx.GuildConfigs.AddAsync(config);
            await ctx.SaveChangesAsync();
        }

        public static async Task AddChannelConfigAsync(this TamamoDbContext ctx, ChannelConfig config)
        {
            await ctx.ChannelConfigs.AddAsync(config);
            await ctx.SaveChangesAsync();
        }

        public static async Task<bool> IsGuildIgnoredAsync(this TamamoDbContext ctx, ulong guildId)
        {
            GuildConfig config = await ctx.GetGuildConfigAsync(guildId);
            return (config == null) ? true : config.IsIgnored;
        }

        public static async Task<bool> IsChannelIgnoredAsync(this TamamoDbContext ctx, ulong channelId)
        {
            ChannelConfig config = await ctx.GetChannelConfigAsync(channelId);
            return (config == null) ? true : config.IsIgnored;
        }

        //public static async Task<bool> IsUserIgnoredAsync(this TamamoDbContext ctx, ulong guildId, ulong userId)
        //{
        //
        //}

        public static async Task<GuildConfig> GetGuildConfigAsync(this TamamoDbContext ctx, ulong guildId)
            => await ctx.GuildConfigs.SingleOrDefaultAsync(gc => gc.GuildId == guildId);

        public static async Task<ChannelConfig> GetChannelConfigAsync(this TamamoDbContext ctx, ulong channelId)
            => await ctx.ChannelConfigs.SingleOrDefaultAsync(ch => ch.ChannelId == channelId);

        /*public static async Task<UserConfig> GetIgnoredUserAsync(this TamamoDbContext ctx, ulong guildId, ulong userId)
        {

        }*/

        public static async Task IgnoreGuildAsync(this TamamoDbContext ctx, ulong guildId)
        {
            GuildConfig config = await ctx.GetGuildConfigAsync(guildId);
            config.IsIgnored = true;
            ctx.Update(config);
            await ctx.SaveChangesAsync();
        }

        public static async Task IgnoreChannelAsync(this TamamoDbContext ctx, ulong channelId)
        {
            ChannelConfig config = await ctx.GetChannelConfigAsync(channelId);
            config.IsIgnored = true;
            ctx.Update(config);
            await ctx.SaveChangesAsync();
        }
    }
}
