using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using TamamoSharp.Utils;

namespace TamamoSharp.Modules
{
    [Name("Conversation")]
    [Summary("Miscellaneous commands that have their own module.")]
    public class ConversationModule : TamamoModuleBase
    {
        private readonly Random _rng;
        private readonly ulong ownerId;

        public ConversationModule(IServiceProvider svc)
        {
            _rng = svc.GetService<Random>();
            ownerId = ulong.Parse((svc.GetService<IConfiguration>())["owner_id"]);
        }

        [Command("say"), Name("Say")]
        [RequireOwner]
        public async Task Say([Remainder] string s)
            => await ReplyAsync(s);

        [Command("test"), Name("Test")]
        [RequireOwner]
        public async Task TestCommand()
        {
            await ReplyAsync($"Count: {Database.GuildConfigs.Count().ToString()}");
        }

        [Command("roll"), Name("Roll")]
        [Summary("Tamamo chooses a number within the specified range.")]
        public async Task Roll(int lower = 0, int upper = 100)
        {
            if (lower > upper)
                await ReplyAsync("Invalid bounds!");
            else
                await ReplyAsync($"You rolled a {_rng.Next(lower, upper)}!");
        }

        [Command("clap"), Name("Clap")]
        [Summary(":clap:")]
        public async Task Clap([Remainder] string input)
            => await ReplyAsync(string.Join(" :clap: ", input.Split(' ')));

        [Command("size"), Name("Size")]
        [Summary("Tamamo choose a random size from 1-32 cm. ( ͡° ͜ʖ ͡°)")]
        public async Task Size()
            => await ReplyAsync((Context.Message.Author.Id == ownerId)
                ? "32 cm. ( ͡° ͜ʖ ͡°)"
                : $"{_rng.Next(0, 31)} cm.");

        [Command("choose"), Name("Choose")]
        [Summary("Tamamo chooses a random option from a comma delimited list.")]
        public async Task Choose([Remainder] string choices)
        {
            string[] split = choices.Split(',');
            if (split.Count() <= 1)
                await ReplyAsync("Nothing to choose!");
            else
                await ReplyAsync(split[_rng.Next(0, split.Count())]);
        }

        [Command("scramble"), Name("Scramble")]
        [Summary("Randomly scrambles a string on a word-by-word basis.")]
        public async Task Scramble([Remainder] string words)
            => await ReplyAsync(String.Join(' ', ((words.Split(' ')).OrderBy(x => _rng.Next())).ToArray()));

        [Command("why")]
        [Summary("asdfasdf")]
        public async Task Why()
            => await ReplyAsync((string)(await WebHelpers.GetJsonResponseAsync("https://api-pandentia.qcx.io/why"))["response"]);
    }
}
