using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TamamoSharp.Services.Logging
{
    public class LoggingService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdsvc;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _discordLogger;
        private readonly ILogger _cmdLogger;

        public LoggingService(DiscordSocketClient client, CommandService cmdsvc, ILoggerFactory loggerFactory)
        {
            _client = client;
            _cmdsvc = cmdsvc;

            _loggerFactory = ConfigureLogger(loggerFactory);
            _discordLogger = _loggerFactory.CreateLogger("discord");
            _cmdLogger = _loggerFactory.CreateLogger("cmd");

            _client.Log += LogDiscord;
            _cmdsvc.Log += LogCommand;
        }

        private ILoggerFactory ConfigureLogger(ILoggerFactory factory)
        {
            factory.AddConsole();
            return factory;
        }

        private Task LogDiscord(LogMessage message)
        {
            _discordLogger.Log(LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString());
            return Task.CompletedTask;
        }

        private Task LogCommand(LogMessage message)
        {
            // Return an error message for async commands
            if (message.Exception is CommandException command)
            {
                // Don't risk blocking the logging task by awaiting a message send; ratelimits!?
                var _ = command.Context.Channel.SendMessageAsync($"Error: {command.Message}");
            }

            _cmdLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
            => (LogLevel)(Math.Abs((int)severity - 5));
    }
}
