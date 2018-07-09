using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TamamoSharp.Database;
using TamamoSharp.Services;
using TamamoSharp.Services.Logging;

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

            _cfg = BuildConfiguration();
            IServiceProvider svc = await BuildServiceProvider();

            svc.GetRequiredService<LoggingService>();
            svc.GetRequiredService<EventHandlerService>();
            await svc.GetRequiredService<CommandHandlerService>().InitializeAsync(svc);

            await _client.LoginAsync(TokenType.Bot, _cfg["tokens:discord"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(GetConfigRoot())
                .AddJsonFile("config.json")
                .Build();
        }

        private async Task<IServiceProvider> BuildServiceProvider()
        {
            TamamoDb tamamoDb = new TamamoDb(_cfg["pg_conn_string"]);
            await tamamoDb.EnsureDbCreated();

            return new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<CommandHandlerService>()
                .AddLogging()
                .AddSingleton<LoggingService>()
                .AddSingleton(tamamoDb)
                .AddSingleton(_cfg)
                .AddSingleton<Random>()
                .AddSingleton<StarboardService>()
                .AddSingleton<EventHandlerService>()
                .AddSingleton<RoslynService>()
                .BuildServiceProvider();
        }

        public static string GetConfigRoot()
        {
            string cwd = Directory.GetCurrentDirectory();
            bool containsSln = Directory.GetFiles(cwd).Any(f => f.Contains("TamamoSharp.sln"));
            return (containsSln) ? cwd : Path.GetFullPath(Path.Combine(cwd, @"..\..\..\"));
        }
    }
}