using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamamoSharp.Module
{
    public class Conversation : ModuleBase<SocketCommandContext>
    {
        private readonly Random _rng;

        public Conversation(Random rng)
        {
            _rng = rng;
        }

        [Command("say")]
        [RequireBotOwner]
        public async Task Say(string s)
            => await ReplyAsync(s);

        [Command("roll")]
        public async Task Roll(int lower = 0, int upper = 100)
        {
            if (lower > upper)
                await ReplyAsync("Invalid bounds!");
            else
                await ReplyAsync($"You rolled a {_rng.Next(lower, upper)}!");
        }

        [Command("choose")]
        public async Task Choose([Remainder] string s)
        {
            string[] choices = s.Split(",");
            if (choices.Count() <= 1)
                await ReplyAsync("Nothing to choose!");
            else
                await ReplyAsync(choices[_rng.Next(0, choices.Count())]);
        }

        [Command("scramble")]
        public async Task Scramble([Remainder] string s)
            => await ReplyAsync(String.Join(" ", ((s.Split(" ")).OrderBy(x => _rng.Next())).ToArray()));
    }
}
