using Discord.Commands;
using System.Linq;

namespace TamamoSharp.Extensions
{
    public static class CommandInfoExtensions
    {
        public static bool CanExecute(this CommandInfo cmd, ICommandContext ctx)
            => (cmd.CheckPreconditionsAsync(ctx)).GetAwaiter().GetResult().IsSuccess;

        public static bool CanExecute(this ModuleInfo m, ICommandContext ctx)
            => m.Commands.Any(x => x.CanExecute(ctx));
    }
}
