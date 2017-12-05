using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using TamamoSharp.Database.GuildConfigs;
using TamamoSharp.Database.Quotes;
using TamamoSharp.Database.Tags;
using TamamoSharp.Services;

namespace TamamoSharp
{
    class Program
    {
        private DiscordSocketClient _client;
        private IConfiguration _cfg;

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });

            _cfg = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();

            IServiceProvider svc = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Verbose
                }))
                .AddSingleton(_cfg)
                .AddSingleton<CommandHandler>()
                .AddSingleton<Logger>()
                .AddSingleton<Random>()
                .AddSingleton<GuildConfigDb>()
                .AddSingleton<QuoteDb>()
                .AddSingleton<TagDb>()
                .AddLogging()
                .BuildServiceProvider();

            svc.GetRequiredService<Logger>();
            await svc.GetRequiredService<CommandHandler>().InitializeAsync(svc);

            await _client.LoginAsync(TokenType.Bot, _cfg["tokens:discord"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}