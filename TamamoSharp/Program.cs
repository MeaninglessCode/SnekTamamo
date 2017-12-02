using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using TamamoSharp.Services;
using Discord.Commands;

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
                .AddSingleton(_cfg)
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Verbose
                }))
                .AddSingleton<CommandHandler>()
                .AddSingleton<Start>()
                .AddLogging()
                .BuildServiceProvider();

            await svc.GetRequiredService<Start>().StartAsync();
            await svc.GetRequiredService<CommandHandler>().InitializeAsync(svc);

            await Task.Delay(-1);
        }
    }
}