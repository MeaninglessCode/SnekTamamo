using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TamamoSharp.Database;

namespace TamamoSharp.Services
{
    public class EventHandlerService
    {
        private readonly DiscordSocketClient _client;
        private readonly TamamoDb _db;
        private readonly StarboardService _sbsvc;

        public EventHandlerService(DiscordSocketClient client, TamamoDb db, StarboardService sbsvc)
        {
            _client = client;
            _db = db;
            _sbsvc = sbsvc;

            _client.GuildAvailable += GuildAvailable;
            _client.ReactionAdded += ReactionAdded;
            _client.ReactionRemoved += ReactionRemoved;
            _client.ReactionsCleared += ReactionsCleared;
            _client.UserJoined += UserJoined;
            _client.UserLeft += UserLeft;
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

        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.Emote.Name == "⭐")
            {
                IUserMessage message = await cache.GetOrDownloadAsync();

                if (message.Source == MessageSource.User)
                    await _sbsvc.HandleStarReaction(message, channel, reaction);
            }
        }

        public async Task ReactionRemoved(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.Emote.Name == "⭐")
            {
                IUserMessage message = await cache.GetOrDownloadAsync();
                await _sbsvc.HandleStarReaction(message, channel, reaction);
            }
        }

        public async Task ReactionsCleared(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel)
        {
            IUserMessage message = await cache.GetOrDownloadAsync();
            StarboardEntry entry = await _db.GetStarboardEntry(message.Id);

            if (entry != null)
            {
                ISocketMessageChannel starboardChannel =
                    _client.GetChannel(entry.StarboardChannelId) as ISocketMessageChannel;
                IMessage botMessage = await starboardChannel.GetMessageAsync(entry.BotMessageId);

                await botMessage.DeleteAsync();
                await _db.DeleteStarboardEntry(entry);
            }
        }

        public async Task UserJoined(SocketGuildUser user)
        {
            GuildConfig config = await _db.GetGuildConfigAsync(user.Guild.Id);

            if (config.JoinMessageEnabled)
            {
                SocketTextChannel joinMessageChannel = 
                    user.Guild.GetTextChannel(config.JoinMessageChannelId);
                await joinMessageChannel.SendMessageAsync(
                    config.JoinMessage.Replace("%user%", user.Username));
            }
            if (config.AutoAssignRole)
            {
                SocketRole autoRole = user.Guild.GetRole(config.AutoAssignRoleId);
                await user.AddRoleAsync(autoRole);
            }
        }

        public async Task UserLeft(SocketGuildUser user)
        {
            GuildConfig config = await _db.GetGuildConfigAsync(user.Guild.Id);

            if (config.LeaveMessageEnabled)
            {
                SocketTextChannel leaveMessageChannel = 
                    user.Guild.GetTextChannel(config.LeaveMessageChannelId);
                await leaveMessageChannel.SendMessageAsync(
                    config.LeaveMessage.Replace("%user%", user.Username));
            }
        }
    }
}
