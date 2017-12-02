using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Threading.Tasks;

namespace TamamoSharp.Services
{
    class Start
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmds;
        private readonly IConfiguration _cfg;

        public Start(DiscordSocketClient client, CommandService cmds, IConfiguration cfg)
        {
            _client = client;
            _cmds = cmds;
            _cfg = cfg;
        }

        public async Task StartAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _cfg["tokens:discord"]);
            await _client.StartAsync();
            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
