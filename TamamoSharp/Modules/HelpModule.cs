using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TamamoSharp.Modules
{
    [Group("help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task AllHelp()
        {
            await ReplyAsync("hello");
        }

        [Command("module")]
        public async Task ModuleHelp()
        {
            await ReplyAsync("Hello");
        }
    }
}
