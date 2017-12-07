using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using TamamoSharp.Database.Quotes;

namespace TamamoSharp.Modules
{
    [Group("quote"), Name("Quote")]
    [Summary("Commands for storing quotes by guild users.")]
    [RequireContext(ContextType.Guild)]
    public class QuotesModule : TamamoModuleBase
    {
        private readonly QuoteDb _qdb;

        public QuotesModule(QuoteDb qdb)
        {
            _qdb = qdb;
        }

        [Command, Name("GetQuote")]
        [Summary("By default, the quote command shows the quote with the given name.")]
        [Priority(0)]
        public async Task GetQuote(string name)
        {
            Quote q = await _qdb.GetQuoteAsync(Context.Guild.Id, name);

            if (!(await BuildEmbedAsync(Context, q)))
                await ReplyAsync("Quote owner not found!");
        }

        [Command("list"), Name("QuoteList"), Alias("l", "ls")]
        [Summary("Lists all quotes available for the current guild.")]
        [Priority(10)]
        public async Task ListQuotes()
        {
            Quote[] quotes = await _qdb.GetQuotesAsync(Context.Guild.Id);

            if (quotes.Length <= 0)
                await DelayDeleteReplyAsync("No quotes found for this guild!");
            else
            {
                string quoteNames = string.Join(", ", from quote in quotes select quote.Name);
                await ReplyAsync($"Quotes:\n{quoteNames}");
            }
        }

        [Command("me"), Name("QuoteMe"), Alias("m", "self")]
        [Summary("Quote yourself with the given content.")]
        [Priority(10)]
        public async Task QuoteMe(string name, [Remainder] string content)
        {
            if (await _qdb.GetQuoteAsync(Context.Guild.Id, name) != null)
            {
                await DelayDeleteReplyAsync($"A quote with the name **{name}** already exists!", 5);
            }

            Quote q = new Quote
            {
                Name = name,
                Content = content,
                OwnerId = Context.Message.Author.Id,
                GuildId = Context.Guild.Id
            };

            await _qdb.AddQuoteAsync(q);
            await DelayDeleteReplyAsync("Quote successfully added!", 5);
        }

        [Command("add"), Name("AddQuote"), Alias("a")]
        [Summary("Add a new quote to the server by passing message IDs separated by a space.")]
        [Priority(10)]
        public async Task AddQuote(string name, [Remainder] string ids)
        {
            string[] messageIds = ids.Split(" ");

            if (messageIds.Length <= 0)
            {
                await DelayDeleteReplyAsync("No IDs found!", 5);
                return;
            }
            else if (await _qdb.GetQuoteAsync(Context.Guild.Id, name) != null)
            {
                await DelayDeleteReplyAsync($"A quote with the name **{name}** already exists!", 5);
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
                        await DelayDeleteReplyAsync("Invalid message ID given!", 5);
                        return;
                    }
                    else
                    {
                        IMessage message = await Context.Channel.GetMessageAsync(messageId);

                        if (message.Author.Id != baseMessage.Author.Id)
                        {
                            await DelayDeleteReplyAsync("Inconsistent message author!", 5);
                            return;
                        }
                        else
                        {
                            double timeDiff = Math.Abs((baseTime - message.Timestamp).TotalSeconds);

                            if (timeDiff > 1800)
                            {
                                await DelayDeleteReplyAsync("Message times too far apart!", 5);
                                return;
                            }
                            else
                                quoteContent += $"\n{message.Content}";
                        }
                    }

                    if (quoteContent.Length > 2000)
                    {
                        await DelayDeleteReplyAsync("Quote too long!", 5);
                        return;
                    }

                    Quote q = new Quote
                    {
                        Name = name,
                        Content = quoteContent,
                        OwnerId = baseMessage.Author.Id,
                        GuildId = Context.Guild.Id
                    };

                    await _qdb.AddQuoteAsync(q);
                    await DelayDeleteReplyAsync("Quote successfully added!", 5);
                }
            }
        }

        [Command("remove"), Name("RemoveQuote"), Alias("r", "d", "delete")]
        [Summary("Removes a quote with the given name from the current guild.")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Priority(10)]
        public async Task RemoveQuote(string name)
        {
            Quote q = await _qdb.GetQuoteAsync(Context.Guild.Id, name);

            if (q == null)
                await DelayDeleteReplyAsync($"Quote **{name}** not found!", 5);
            else
            {
                await _qdb.DeleteQuoteAsync(q);
                await DelayDeleteReplyAsync("Quote deleted!", 5);
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

            var builder = new EmbedBuilder
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

            await ctx.Channel.SendMessageAsync("", embed: builder.Build());
            await _qdb.AddUseAsync(q);

            return true;
        }
    }
}
