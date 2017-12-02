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

        [Command("scramble")]
        public async Task Scramble(string s)
        {
            await ReplyAsync("do it later");
        }
    }
}
