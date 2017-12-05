using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TamamoSharp.Database.GuildConfigs;

namespace TamamoSharp.Modules
{
    [Name("Administration")]
    [Summary("Commands for performing administrative actions within a guild.")]
    public class AdministrationModule : TamamoModuleBase
    {
        [Group("prune")]
        [Summary("Several methods of pruning messages from a guild.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public class Prune : TamamoModuleBase
        {
            [Command]
            [Priority(0)]
            public async Task PruneSelf(int num = 100)
                => await DoPrune(Context, x => x.Author == Context.User, num);

            [Command("user")]
            [Priority(10)]
            public async Task PruneUser(SocketGuildUser user, int num = 100)
                => await DoPrune(Context, x => x.Author == user, num);

            [Command("all")]
            [Priority(10)]
            public async Task PruneAll(int num = 100)
                => await DoPrune(Context, x => true, num);

            [Command("embeds")]
            [Priority(10)]
            public async Task PruneEmbeds(int num = 100)
                => await DoPrune(Context, x => x.Embeds.Count > 0, num);

            [Command("files")]
            public async Task PruneFiles(int num = 100)
                => await DoPrune(Context, x => x.Attachments.Count > 0, num);

            [Command("contains")]
            public async Task PruneSubstring(string subString, int num = 100)
                => await DoPrune(Context, x => x.Content.Contains(subString), num);

            [Command("bot")]
            [Priority(10)]
            public async Task PruneBot(int num = 100)
                => await DoPrune(Context, x => x.Author.IsBot, num);

            private async Task DoPrune(SocketCommandContext ctx, Func<IMessage, bool> expr, int count = 100)
            {
                ITextChannel channel = ctx.Channel as ITextChannel;
                IEnumerable<IMessage> toDelete = (await channel.GetMessagesAsync(count).Flatten()).Where(expr);

                try
                {
                    await channel.DeleteMessagesAsync(toDelete);
                }
                catch (Discord.Net.HttpException e)
                {
                    if (e.DiscordCode == 403)
                        await ctx.Channel.SendMessageAsync($"I don't have permission to do that!");
                    else
                        await ctx.Channel.SendMessageAsync($"{e}");
                }

                int deleted = toDelete.Count();
                List<string> result = new List<string>
                {
                    $"{deleted} message{((deleted == 1) ? " was" : "'s were")} pruned!"
                };

                if (deleted > 0)
                {
                    result.Add(" ");
                    Dictionary<string, int> authors = toDelete.GroupBy(x => $"{x.Author.Username}#{x.Author.Discriminator}")
                        .ToDictionary(x => x.Key, x => x.Count());

                    foreach (var i in authors)
                        result.Add($"{i.Key}: {i.Value}");
                }

                string toSend = string.Join("\n", result);
                if (toSend.Length > 2000)
                    await DelayDeleteReplyAsync($"Pruned {deleted} messages!", 5);
                else
                    await DelayDeleteReplyAsync(toSend, 5);
            }
        }

        [Group("ignore")]
        public class Ignore : TamamoModuleBase
        {
            private readonly GuildConfigDb _gcdb;

            public Ignore(GuildConfigDb gcdb)
            {
                _gcdb = gcdb;
            }
        }


        [Command("kick")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Kick(SocketGuildUser user, string reason = null)
        {
            await user.KickAsync(reason);
            await ReplyAsync($"{user.Username} has been kicked.");
        }

        [Command("softban")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task SoftBan(SocketGuildUser user, string reason = null)
        {
            await Context.Guild.AddBanAsync(user, reason: reason);
            await Context.Guild.RemoveBanAsync(user);
            await ReplyAsync(":ok_hand:");
        }
    }
}
