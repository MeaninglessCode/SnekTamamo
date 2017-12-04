using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TamamoSharp.Modules
{
    [Group("help"), Name("Help")]
    [Summary("Various commands for help with other commands and modules.")]
    public class HelpModule : TamamoModuleBase
    {
        private readonly IServiceProvider _svc;
        private readonly CommandService _cmds;

        public HelpModule(IServiceProvider svc, CommandService cmds)
        {
            _svc = svc;
            _cmds = cmds;
        }

        [Command]
        public async Task AllHelp()
        {
            IEnumerable<ModuleInfo> modules = _cmds.Modules.Where(x =>
                !string.IsNullOrWhiteSpace(x.Summary));

            EmbedBuilder builder = new EmbedBuilder()
                .WithFooter(x => x.Text = $"Type `@Tamamo help <module>` for more info");

            foreach (ModuleInfo m in modules)
            {   
                bool success = false;
                foreach (CommandInfo cmd in m.Commands)
                {
                    PreconditionResult result = await cmd
                        .CheckPreconditionsAsync(Context, _svc);

                    if (result.IsSuccess)
                    {
                        success = true;
                        break;
                    }
                }

                if (!success)
                    continue;

                builder.AddField(m.Name, m.Summary);
                Console.WriteLine(m.Name);
                Console.WriteLine(m.Summary);
            }

            await DMReplyAsync("", embed: builder.Build());
        }

        [Command]
        public async Task ModuleHelp(string moduleName)
        {
            ModuleInfo module = _cmds.Modules.FirstOrDefault(x =>
                x.Name.ToLower() == moduleName.ToLower());

            if (module == null)
            {
                await DelayDeleteReplyAsync($"Module {moduleName} not found!", 3);
                return;
            }

            IEnumerable<CommandInfo> cmds = module.Commands.Where(x =>
                !string.IsNullOrWhiteSpace(x.Summary)).GroupBy(x => x.Name)
                .Select(x => x.First());

            if (cmds.Count() <= 0)
            {
                await DelayDeleteReplyAsync("no commands", 3);
                return;
            }

            await ReplyAsync("Hello");
        }

        [Command]
        public async Task CommandHelp(string moduleName, string cmdName)
        {
            IEnumerable<ModuleInfo> module = _cmds.Modules.Where(x =>
                x.Name.ToLower() == moduleName.ToLower());

            if (module == null)
            {
                await ReplyAsync("nf");
                return;
            }
        }
    }
}
