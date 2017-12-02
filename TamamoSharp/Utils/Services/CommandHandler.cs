using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using Discord;
using System.Threading.Tasks;
using System.Reflection;

namespace TamamoSharp.Services
{
    class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmds;
        private readonly IConfiguration _cfg;
        private IServiceProvider _svc;
        private int argPos = 0;

        public CommandHandler(IServiceProvider svc, CommandService cmds, DiscordSocketClient client, IConfiguration cfg)
        {
            _cfg = cfg;
            _cmds = cmds;
            _client = client;
            _svc = svc;

            _client.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider svcprovider)
        {
            _svc = svcprovider;
            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task MessageReceived(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || (msg.Source != MessageSource.User))
                return;
            else if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                SocketCommandContext ctx = new SocketCommandContext(_client, msg);
                IResult result = await _cmds.ExecuteAsync(ctx, argPos, _svc);

                if (result.Error.HasValue && (result.Error.Value != CommandError.UnknownCommand))
                    await ctx.Channel.SendMessageAsync(result.ToString());
                else if (result.Error.HasValue)
                    await ctx.Channel.SendMessageAsync(result.ToString());
            }
        }
    }
}
