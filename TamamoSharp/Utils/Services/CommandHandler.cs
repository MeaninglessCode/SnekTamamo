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
        private readonly CommandService _cmdsvc;
        private readonly IConfiguration _cfg;
        private IServiceProvider _svcprovider;
        private int argPos = 0;

        public CommandHandler(IServiceProvider svcprovider, CommandService cmdsvc, DiscordSocketClient client, IConfiguration cfg)
        {
            _cfg = cfg;
            _cmdsvc = cmdsvc;
            _client = client;
            _svcprovider = svcprovider;

            _client.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _svcprovider = provider;
            await _cmdsvc.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task MessageReceived(SocketMessage sMsg)
        {
            if (!(sMsg is SocketUserMessage msg) || (msg.Source != MessageSource.User))
                return;
            else if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                SocketCommandContext ctx = new SocketCommandContext(_client, msg);
                IResult result = await _cmdsvc.ExecuteAsync(ctx, argPos, _svcprovider);

                if (result.Error.HasValue && (result.Error.Value != CommandError.UnknownCommand))
                    await ctx.Channel.SendMessageAsync(result.ToString());
                else if (result.Error.HasValue)
                    await ctx.Channel.SendMessageAsync(result.ToString());
            }
        }
    }
}
