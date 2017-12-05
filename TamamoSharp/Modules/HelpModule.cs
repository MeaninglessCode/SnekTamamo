using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        [Command, Name("ModuleHelp")]
        [Summary("Shows a list of available modules for use with the other help commands.")]
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

        [Command, Name("ModuleCommands")]
        [Summary("Lists all commands from a particular module.")]
        public async Task ModuleHelp(string moduleName)
        {
            ModuleInfo module = _cmds.Modules.FirstOrDefault(x =>
                x.Name.ToLower() == moduleName.ToLower());

            if (module == null)
            {
                await DelayDeleteReplyAsync($"Module `{moduleName}` not found!", 5);
                return;
            }

            IEnumerable<CommandInfo> cmds = module.Commands.Where(x =>
                !string.IsNullOrWhiteSpace(x.Summary)).GroupBy(x => x.Name)
                .Select(x => x.First());

            if (cmds.Count() <= 0)
            {
                await DelayDeleteReplyAsync($"No commands found for module `{moduleName}`!", 5);
                return;
            }

            EmbedBuilder builder = new EmbedBuilder()
                .WithFooter(x => x.Text = $"Type `@Tamamo help <command>` for more info");

            foreach (CommandInfo cmd in cmds)
            {
                PreconditionResult result = await cmd
                    .CheckPreconditionsAsync(Context, _svc);
                if (result.IsSuccess)
                    builder.AddField(cmd.Name, cmd.Summary);
            }

            await ReplyAsync("", embed: builder.Build());
        }

        [Command, Name("CommandHelp")]
        [Summary("Gets help for a specific command within a module.")]
        public async Task CommandHelp(string moduleName, string cmdName)
        {
            ModuleInfo module = _cmds.Modules.FirstOrDefault(x =>
                x.Name.ToLower() == moduleName.ToLower());

            if (module == null)
            {
                await ReplyAsync("nf");
                return;
            }

            IEnumerable<CommandInfo> cmds = module.Commands
                .Where(x => !string.IsNullOrWhiteSpace(x.Summary));

            if (cmds.Count() <= 0)
            {
                await DelayDeleteReplyAsync($"No commands found for module `{moduleName}`!", 5);
                return;
            }

            string alias = $"{moduleName} {cmdName}".ToLower();
            IEnumerable<CommandInfo> cmd = cmds.Where(x => x.Aliases.Contains(alias));
            EmbedBuilder builder = new EmbedBuilder();
            List<string> aliases = new List<string>();

            foreach (CommandInfo info in cmd)
            {
                PreconditionResult result = await info.CheckPreconditionsAsync(Context, _svc);
                if (result.IsSuccess)
                {
                    StringBuilder sb = new StringBuilder()
                        .Append($"@Tamamo {info.Aliases.First()}");

                    foreach (ParameterInfo pInfo in info.Parameters)
                    {
                        if (pInfo.IsRemainder)
                            sb.Append($" {pInfo.Name}...");
                        if (pInfo.IsOptional)
                            sb.Append($" [{pInfo.Name}]");
                        else
                            sb.Append($" <{pInfo.Name}>");
                    }
                    builder.AddField(sb.ToString(), info.Summary);
                }
                aliases.AddRange(info.Aliases);
            }

            builder.WithFooter(x => x.Text = $"Aliases: {string.Join(", ", aliases)}");
            await ReplyAsync("", embed: builder.Build());
        }
    }
}
