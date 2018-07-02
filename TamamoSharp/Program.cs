using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TamamoSharp.Database;
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

            _cfg = BuildConfiguration();
            IServiceProvider svc = BuildServiceProvider();

            svc.GetRequiredService<Logger>();
            await svc.GetRequiredService<CommandHandler>().InitializeAsync(svc);

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

        private IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton(_cfg)
                .AddSingleton<CommandHandler>()
                .AddSingleton<Logger>()
                .AddSingleton<Random>()
                .AddDbContextPool<TamamoDbContext>(options =>
                {
                    options.UseNpgsql(_cfg["pg_conn_string"]);
                })
                .AddSingleton<RoslynService>()
                .AddLogging()
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