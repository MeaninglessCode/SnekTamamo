using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using TamamoSharp.Database;
using TamamoSharp.Utils;

namespace TamamoSharp.Services
{
    public class CommandHandlerService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmds;
        private readonly IConfiguration _cfg;
        private IServiceProvider _svc;
        private readonly TamamoDb _db;
        private int argPos = 0;

        private ConcurrentDictionary<ulong, string> _prefixes { get; } = new ConcurrentDictionary<ulong, string>();

        public CommandHandlerService(IServiceProvider svc, CommandService cmds, DiscordSocketClient client,
            IConfiguration cfg, TamamoDb db, StarboardService sbsvc)
        {
            _cfg = cfg;
            _cmds = cmds;
            _client = client;
            _svc = svc;
            _db = db;

            _client.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider svcprovider)
        {
            _svc = svcprovider;

            _cmds.AddTypeReader<CommandInfo>(new CommandInfoTypeReader());
            _cmds.AddTypeReader<ModuleInfo>(new ModuleInfoTypeReader());

            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task MessageReceived(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || (msg.Source != MessageSource.User))
                return;
            else if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                if (!(msg.Channel is SocketDMChannel))
                {
                    SocketGuildChannel c = (msg.Channel) as SocketGuildChannel;
                    GuildIgnoreData ignoreData = await _db.GetGuildIgnoreDataAsync(c.Guild.Id);

                    if (ignoreData.IsGuildIgnored)
                        return;
                    else if (ignoreData.ChannelData[c.Id] == true)
                        return;
                    //else if (await _db.UserIgnoredAsync(c.Guild.Id, msg.Author.Id))
                    //    return;
                }
                
                SocketCommandContext ctx = new SocketCommandContext(_client, msg);
                IResult result = await _cmds.ExecuteAsync(ctx, argPos, _svc);

                if (result.Error.HasValue)
                {
                    switch (result.Error)
                    {
                        case CommandError.UnknownCommand:
                            break;
                        default:
                            await ctx.Channel.SendMessageAsync(result.ToString());
                            break;
                    }
                }
                else if (result.Error.HasValue)
                    await ctx.Channel.SendMessageAsync(result.ToString());
            }
        }
    }
}
