using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using TamamoSharp.Database.Quotes;

namespace TamamoSharp.Module
{
    [Group("quote")]
    [RequireContext(ContextType.Guild)]
    public class QuotesModule : ModuleBase<SocketCommandContext>
    {
        private readonly QuoteDb _qdb;

        public QuotesModule(QuoteDb qdb)
        {
            _qdb = qdb;
        }

        [Command]
        [Priority(0)]
        public async Task GetQuote(string name)
        {
            Quote q = await _qdb.GetQuoteAsync(Context.Guild.Id, name);

            if (!(await BuildEmbedAsync(Context, q)))
                await ReplyAsync("Quote owner not found!");
        }

        [Command("list"), Alias("l", "ls")]
        [Priority(10)]
        public async Task ListQuotes()
        {
            Quote[] quotes = await _qdb.GetQuotesAsync(Context.Guild.Id);

            if (quotes.Count() == 0)
                await ReplyAsync("No quotes found for this guild!");
            else
            {
                string quoteNames = string.Join(",", from quote in quotes select quote.Name);
                await ReplyAsync($"Quotes:\n{quoteNames}");
            }
        }

        [Command("me"), Alias("m", "self")]
        [Priority(10)]
        public async Task QuoteMe(string name, string content)
        {
            Quote q = new Quote
            {
                Name = name,
                Content = content,
                OwnerId = Context.Message.Author.Id,
                GuildId = Context.Guild.Id
            };

            bool result = await _qdb.AddQuoteAsync(q);

            if (!result)
                await ReplyAsync("A quote with that name already exists!");
            else
                await ReplyAsync("Quote successfully added!");
        }

        [Command("add"), Alias("a")]
        [Priority(10)]
        public async Task AddQuote(string name, [Remainder] string ids)
        {
            string[] messageIds = ids.Split(" ");

            if (messageIds.Count() <= 0)
            {
                await ReplyAsync("No IDs found!");
                return;
            }
            else if (await _qdb.ExistsAsync(Context.Guild.Id, name))
            {
                await ReplyAsync("A quote with that name already exists!");
                return;
            }
            else
            {
                if (!ulong.TryParse(messageIds[0], out ulong temp))
                {
                    await ReplyAsync("Invalid message ID given!");
                    return;
                }

                IMessage baseMessage = await Context.Channel.GetMessageAsync(temp);
                ulong authorId = baseMessage.Author.Id;
                DateTimeOffset baseTime = baseMessage.Timestamp;
                string quoteContent = "";

                foreach (string id in messageIds)
                {
                    if (!ulong.TryParse(id, out ulong messageId))
                    {
                        await ReplyAsync("Invalid message ID given!");
                        return;
                    }
                    else
                    {
                        IMessage message = await Context.Channel.GetMessageAsync(messageId);

                        if (message.Author.Id != baseMessage.Author.Id)
                        {
                            await ReplyAsync("Inconsistent message author!");
                            return;
                        }
                        else
                        {
                            double timeDiff = Math.Abs((baseTime - message.Timestamp).TotalSeconds);

                            if (timeDiff > 1800)
                            {
                                await ReplyAsync("Message times too far apart!");
                            }
                            else
                                quoteContent += $"\n{message.Content}";
                        }
                    }

                    if (quoteContent.Length > 1800)
                    {
                        await ReplyAsync("Quote too long!");
                        return;
                    }

                    Quote q = new Quote
                    {
                        Name = name,
                        Content = quoteContent,
                        OwnerId = baseMessage.Author.Id,
                        GuildId = Context.Guild.Id
                    };

                    bool result = await _qdb.AddQuoteAsync(q);

                    if (!result)
                        await ReplyAsync("Quote failed to add!");
                    else
                        await ReplyAsync("Quote successfully added!");
                }
            }
        }

        [Command("remove"), Alias("r", "d", "delete")]
        [Priority(10)]
        public async Task RemoveQuote(string name)
        {
            Quote q = await _qdb.GetQuoteAsync(Context.Guild.Id, name);

            if (q == null)
                await ReplyAsync("Quote not found!");
            else
            {
                bool result = await _qdb.DeleteQuoteAsync(q);

                if (!result)
                    await ReplyAsync("Failed to remove quote!");
                else
                    await ReplyAsync("Quote removed!");
            }
        }

        private async Task<bool> BuildEmbedAsync(SocketCommandContext ctx, Quote q)
        {
            SocketGuild guild = ctx.Guild;
            SocketGuildUser user = (await ctx.Channel.GetUserAsync(q.OwnerId)) as SocketGuildUser;

            if (user is null)
                return false;

            string authorName = $"{user.Username}#{user.Discriminator}";
            DateTime timeStamp = q.CreatedAt;

            if (user.Nickname != null)
                authorName += $" (aka {user.Nickname})";

            var e = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = authorName,
                    IconUrl = user.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = q.CreatedAt.ToString("ddd, MMM d, yyyy h:mm:ss tt")
                },
                Description = q.Content
            };

            await ctx.Channel.SendMessageAsync("", false, e.Build());
            return true;
        }
    }
}
