using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TamamoSharp.Services
{
    class Logger
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmds;

        public Logger(DiscordSocketClient client, CommandService cmds)
        {
            _client = client;
            _cmds = cmds;

            _client.Log += LogAsync;
            _client.MessageReceived += LogMessageAsync;
            _cmds.Log += LogAsync;
        }

        public Task LogAsync(LogMessage msg)
        {
            Console.OutputEncoding = Encoding.Unicode;
            StringBuilder output = new StringBuilder();

            output.Append($"[{DateTime.Now.ToString("hh:mm:ss")}] ");
            output.Append($"({msg.Source}) {msg.Message}");

            Console.Out.WriteLineAsync(output.ToString());
            return Task.CompletedTask;
        }

        public Task LogMessageAsync(SocketMessage msg)
        {
            Console.OutputEncoding = Encoding.Unicode;
            StringBuilder output = new StringBuilder();
            
            output.Append($"[{DateTime.Now.ToString("hh:mm:ss")}] ");

            if (msg.Channel is IDMChannel)
                output.Append($"({msg.Source}) <Direct> ");
            else if (msg.Channel is ITextChannel)
                output.Append($"({msg.Source}) <{(msg.Channel as ITextChannel).Guild.Name}#{msg.Channel.Name}> ");
            output.Append($"({msg.Author.Username}#{msg.Author.Discriminator}): {msg.Content}");

            Console.Out.WriteLineAsync(output.ToString());
            return Task.CompletedTask;
        }
    }
}
