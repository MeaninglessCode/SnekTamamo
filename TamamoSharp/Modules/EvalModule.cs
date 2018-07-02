using Discord.Commands;
using System.Threading.Tasks;
using TamamoSharp.Services;

namespace TamamoSharp.Modules
{
    [Group("eval"), Name("Eval")]
    [RequireOwner]
    public class EvalModule : TamamoModuleBase
    {
        private readonly RoslynService _roslyn;

        public EvalModule(RoslynService roslyn)
        {
            _roslyn = roslyn;
        }

        [Command(RunMode = RunMode.Async)]
        public async Task EvalAsync([Remainder] string code)
            => await ReplyAsync("", embed: await _roslyn.ExecuteAsync(Context, code));
    }
}
