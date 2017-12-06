using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TamamoSharp.Modules
{
    [Name("Conversation")]
    [Summary("Miscellaneous commands that have their own module.")]
    public class ConversationModule : ModuleBase<SocketCommandContext>
    {
        private readonly Random _rng;
        private readonly ulong ownerId;

        public ConversationModule(IServiceProvider svc)
        {
            _rng = svc.GetService<Random>();
            ownerId = ulong.Parse((svc.GetService<IConfiguration>())["owner_id"]);
        }

        [Command("say"), Name("Say")]
        [RequireBotOwner]
        public async Task Say(string s)
            => await ReplyAsync(s);

        [Command("roll"), Name("Roll")]
        [Summary("Tamamo chooses a number within the specified range.")]
        public async Task Roll(int lower = 0, int upper = 100)
        {
            if (lower > upper)
                await ReplyAsync("Invalid bounds!");
            else
                await ReplyAsync($"You rolled a {_rng.Next(lower, upper)}!");
        }

        [Command("size"), Name("Size")]
        [Summary("Tamamo choose a random size from 1-32 cm. ( ͡° ͜ʖ ͡°)")]
        public async Task Size()
            => await ReplyAsync((Context.Message.Author.Id == ownerId)? "32 cm. ( ͡° ͜ʖ ͡°)"
                : $"{_rng.Next(0, 31)} cm.");

        [Command("choose"), Name("Choose")]
        [Summary("Tamamo chooses a random option from a comma delimited list.")]
        public async Task Choose([Remainder] string s)
        {
            string[] choices = s.Split(',');
            if (choices.Count() <= 1)
                await ReplyAsync("Nothing to choose!");
            else
                await ReplyAsync(choices[_rng.Next(0, choices.Count())]);
        }

        [Command("scramble"), Name("Scramble")]
        [Summary("Randomly scrambles a string on a word-by-word basis.")]
        public async Task Scramble([Remainder] string s)
            => await ReplyAsync(String.Join(' ', ((s.Split(' ')).OrderBy(x => _rng.Next())).ToArray()));
    }
}
