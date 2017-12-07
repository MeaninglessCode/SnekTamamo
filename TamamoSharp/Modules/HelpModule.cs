using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TamamoSharp.Extensions;

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
        [Priority(0)]
        public async Task AllHelp()
        {
            IEnumerable<ModuleInfo> modules = _cmds.Modules.Where(x => 
                !string.IsNullOrWhiteSpace(x.Summary));

            EmbedBuilder builder = new EmbedBuilder()
                .WithFooter(x => x.Text = "Type @Tamamo help <module> for more info");

            foreach (ModuleInfo m in modules)
                builder.AddField(m.Name, m.Summary);
            await DMReplyAsync("", embed: builder.Build());
        }

        [Command, Name("CommandHelp"), Alias("command", "cmd", "c")]
        [Summary("Gets help for a specific command within a module.")]
        [Priority(10)]
        public async Task CommandHelp([Remainder] CommandInfo cmdName)
        {
            EmbedBuilder builder = await GetCommandInfoEmbed(cmdName);
            await DMReplyAsync("", embed: builder.Build());
        }

        [Command, Name("ModuleCommands"), Alias("module", "mod", "m")]
        [Summary("Lists all commands from a particular module.")]
        [Priority(10)]
        public async Task ModuleHelp([Remainder] ModuleInfo modName)
        {
            IEnumerable<CommandInfo> cmds = modName.Commands.Where(x =>
                !string.IsNullOrWhiteSpace(x.Summary) && x.CanExecute(Context));

            EmbedBuilder builder = new EmbedBuilder()
                .WithFooter(x => x.Text = "Type @Tamamo help <command> for more info!");

            foreach (CommandInfo cmd in cmds)
                builder.AddField(cmd.Name, cmd.Summary);

            await DMReplyAsync("", embed: builder.Build());
        }

        private Task<EmbedBuilder> GetCommandInfoEmbed(CommandInfo cmd)
        {
            string aliases = string.Join(" | ", cmd.Aliases);

            StringBuilder sb = new StringBuilder();
            foreach (ParameterInfo p in cmd.Parameters)
            {
                if (p.IsOptional)
                    sb.Append($" [{p.Name} = {p.DefaultValue}{(p.IsRemainder ? "...]" : "]")}");
                else
                    sb.Append($" <{p.Name}{(p.IsRemainder ? "...>" : ">")}");
            }

            EmbedBuilder builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $"{cmd.Name} (In Module: {cmd.Module.Name})"
                },
                Description = $"**Usage**:\n{Context.Client.CurrentUser.Mention} "
                + $"{(cmd.Aliases.Count > 1 ? $"({aliases})" : aliases)}{sb.ToString()}"
                + $"{(!string.IsNullOrWhiteSpace(cmd.Summary) ? $"\n\n**Summary**:\n{cmd.Summary}" : "")}"
            };

            return Task.FromResult(builder);
        }
    }
}
