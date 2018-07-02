using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using Discord;
using System.Threading.Tasks;
using System.Reflection;
using TamamoSharp.Utils;
using TamamoSharp.Database;

namespace TamamoSharp.Services
{
    class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmds;
        private readonly IConfiguration _cfg;
        private IServiceProvider _svc;
        private TamamoDbContext _db;
        private int argPos = 0;

        public CommandHandler(IServiceProvider svc, CommandService cmds, DiscordSocketClient client, IConfiguration cfg, TamamoDbContext db)
        {
            _cfg = cfg;
            _cmds = cmds;
            _client = client;
            _svc = svc;
            _db = db;

            _client.MessageReceived += MessageReceived;
            _client.GuildAvailable += GuildAvailable;
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

                    if (await _db.IsGuildIgnoredAsync(c.Guild.Id))
                        return;
                    else if (await _db.IsChannelIgnoredAsync(c.Id))
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

        public async Task GuildAvailable(SocketGuild guild)
        {
            GuildConfig gc = await _db.GetGuildConfigAsync(guild.Id);
            if (gc == null)
            {
                await _db.AddGuildConfigAsync(new GuildConfig
                {
                    GuildId = guild.Id
                });

                foreach (IGuildChannel ch in guild.Channels)
                {
                    await _db.AddChannelConfigAsync(new ChannelConfig
                    {
                        ChannelId = ch.Id,
                        GuildId = guild.Id
                    });
                }
            }
        }
    }
}
