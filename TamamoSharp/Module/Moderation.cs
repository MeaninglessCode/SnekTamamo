using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamamoSharp.Module
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        [Group("prune")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public class Prune : ModuleBase<SocketCommandContext>
        {
            [Command]
            [Priority(0)]
            public async Task PruneSelf(int num = 100)
            {
                await DoPrune(x => x.Author == Context.User, Context);
            }

            [Command("user")]
            [Priority(10)]
            public async Task PruneUser(SocketGuildUser user, int num = 100)
            {
                await DoPrune(x => x.Author == user, Context, num);
            }

            [Command("all")]
            [Priority(10)]
            public async Task PruneAll(int num = 100)
            {
                await DoPrune(x => true, Context, num);
            }

            private async Task DoPrune(Func<IMessage, bool> expr, SocketCommandContext ctx, int count = 100)
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
                        await ctx.Channel.SendMessageAsync($"I don't have permission to do that !");
                    else
                        await ctx.Channel.SendMessageAsync($"{e}");
                }

                int deleted = toDelete.Count();

                if (deleted != 0)
                {

                }
            }
        }
    }
}
