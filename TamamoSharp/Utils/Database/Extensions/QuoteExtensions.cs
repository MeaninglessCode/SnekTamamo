using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TamamoSharp.Database
{
    public static class QuoteExtensions
    {
        public static async Task AddQuoteAsync(this TamamoDbContext ctx, Quote quote)
        {
            await ctx.Quotes.AddAsync(quote);
            await ctx.SaveChangesAsync();
        }

        public static async Task DeleteQuoteAsync(this TamamoDbContext ctx, Quote quote)
        {
            ctx.Remove(quote);
            await ctx.SaveChangesAsync();
        }

        public static async Task<Quote> GetQuoteAsync(this TamamoDbContext ctx, ulong guildId, string name)
            => await ctx.Quotes.FirstOrDefaultAsync(q => q.GuildId == guildId && q.Name == name);

        public static async Task<Quote[]> GetAllQuotesAsync(this TamamoDbContext ctx, ulong guildId)
            => await ctx.Quotes.Where(q => q.GuildId == guildId).ToArrayAsync();

        public static async Task AddQuoteUseAsync(this TamamoDbContext ctx, Quote quote)
        {
            quote.Uses += 1;
            ctx.Update(quote);
            await ctx.SaveChangesAsync();
        }
    }
}
