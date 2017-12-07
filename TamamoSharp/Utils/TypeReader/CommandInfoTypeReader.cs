using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using TamamoSharp.Extensions;

namespace TamamoSharp.Utils
{
    public class CommandInfoTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext ctx, string input, IServiceProvider svc)
        {
            CommandService cmdsvc = svc.GetRequiredService<CommandService>();
            CommandInfo cmd = cmdsvc.Commands.FirstOrDefault(x => x.CanExecute(ctx) &&
                x.Aliases.Any(y => string.Equals(y, input, StringComparison.OrdinalIgnoreCase)));

            if (cmd == null)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Command not found!"));
            return Task.FromResult(TypeReaderResult.FromSuccess(cmd));
        }
    }
}
