using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TamamoSharp.Modules
{
    public class AdministrationModule : TamamoModuleBase
    {
        [Group("prune")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public class Prune : TamamoModuleBase
        {
            [Command]
            [Priority(0)]
            public async Task PruneSelf(int num = 100)
            {
                await DoPrune(Context, x => x.Author == Context.User, num);
            }

            [Command("user")]
            [Priority(10)]
            public async Task PruneUser(SocketGuildUser user, int num = 100)
            {
                await DoPrune(Context, x => x.Author == user, num);
            }

            [Command("all")]
            [Priority(10)]
            public async Task PruneAll(int num = 100)
            {
                await DoPrune(Context, x => true, num);
            }

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

                if (deleted != 0)
                {

                }
            }
        }

        [Command("kick")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Kick(SocketGuildUser user, string reason = null)
        {
            await user.KickAsync(reason);
            await ReplyAsync($"{user.Username} has been kicked.");
        }

    }
}
