using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TamamoSharp.Module
{
    [Group("help")]
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task AllHelp()
        {
            await ReplyAsync("hello");
        }

        [Command("module")]
        public async Task HelpModule()
        {
            await ReplyAsync("Hello");
        }
    }
}
