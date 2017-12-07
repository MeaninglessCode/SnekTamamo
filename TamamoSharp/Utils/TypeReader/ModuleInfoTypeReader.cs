using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using TamamoSharp.Extensions;

namespace TamamoSharp.Utils
{
    public class ModuleInfoTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext ctx, string input, IServiceProvider svc)
        {
            CommandService cmdsvc = svc.GetRequiredService<CommandService>();
            ModuleInfo module = cmdsvc.Modules.FirstOrDefault(x => x.CanExecute(ctx) &&
                string.Equals(x.Name, input, StringComparison.OrdinalIgnoreCase));

            if (module == null)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Module not found!"));
            return Task.FromResult(TypeReaderResult.FromSuccess(module));
        }
    }
}
