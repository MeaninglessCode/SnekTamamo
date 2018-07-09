using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using TamamoSharp.Database;

namespace TamamoSharp.Services
{
    public class StarboardService
    {
        private readonly DiscordSocketClient _client;
        private readonly TamamoDb _db;
        private readonly string[] starEmotes = { "⭐", "🌟", "💫", "✨", "🎇", "🎆" };

        public StarboardService(DiscordSocketClient client, TamamoDb db)
        {
            _client = client;
            _db = db;
        }

        public async Task HandleStarReaction(IUserMessage message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (!(message.Channel is SocketDMChannel))
            {
                SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
                GuildConfig config = await _db.GetGuildConfigAsync(guildChannel.Guild.Id);

                if (config.StarboardChannelId == 0)
                    return;
                else if (message.Reactions.Count <= 0 || !message.Reactions.ContainsKey(reaction.Emote))
                {
                    StarboardEntry entry = await _db.GetStarboardEntry(message.Id);

                    ISocketMessageChannel starboardChannel =
                        _client.GetChannel(config.StarboardChannelId) as ISocketMessageChannel;

                    IMessage botMessage = await starboardChannel.GetMessageAsync(entry.BotMessageId);
                    await botMessage.DeleteAsync();
                    await _db.DeleteStarboardEntry(entry);
                }
                else if (config.StarboardEnabled &&
                    message.Reactions[reaction.Emote].ReactionCount >= config.StarboardThreshold)
                {
                    if (!(_client.GetChannel(config.StarboardChannelId) is ISocketMessageChannel starboardChannel))
                    {
                        message = await channel.SendMessageAsync("Starboard configured incorrectly!");
                        await Task.Delay(5000);
                        await message.DeleteAsync();
                        return;
                    }

                    StarboardEntry entry = await _db.GetStarboardEntry(message.Id);

                    if (entry == null)
                        await CreateNewStarboardEntry(message, starboardChannel,
                            reaction, config);
                    else
                        await UpdateStarboardEntry(message, starboardChannel, entry,
                            reaction, config.StarboardThreshold);
                }
            }
        }

        public async Task CreateNewStarboardEntry(IUserMessage message, ISocketMessageChannel starboardChannel,
            SocketReaction reaction, GuildConfig config)
        {
            int starCount = message.Reactions[reaction.Emote].ReactionCount;

            EmbedAuthorBuilder author = new EmbedAuthorBuilder
            {
                Name = $"{message.Author.Username}#{message.Author.Discriminator}",
                IconUrl = message.Author.GetAvatarUrl()
            };

            EmbedBuilder builder = new EmbedBuilder
            {
                Author = author,
                Color = GetStarColor(starCount),
                Timestamp = message.Timestamp
            };

            if (message.Attachments.Count == 1)
            {
                IAttachment attachment = message.Attachments.ElementAt(0);
                builder.Description = message.Content;
                builder.ImageUrl = attachment.Url;
            }
            else if (message.Attachments.Count >= 2)
            {
                string embedContent = "";
                foreach (Attachment attachment in message.Attachments)
                    embedContent += $"{attachment.Url}\n";
                builder.Description = $"{message.Content}\n{embedContent}";
            }
            else if (message.Embeds.Count == 1)
            {
                // TODO: Handle embeds
                Console.WriteLine("unhandled embed!");
            }
            else if (message.Embeds.Count >= 2)
            {

            }
            else
                builder.Description = message.Content;

            RestUserMessage botMessage = await starboardChannel.SendMessageAsync(
                $"{GetStarLevel(starCount, config.StarboardThreshold)} **{starCount}** <#{message.Channel.Id}> " +
                $"[ID: {message.Id}]", embed: builder.Build());

            SocketGuildChannel guildChannel = starboardChannel as SocketGuildChannel;

            StarboardEntry entry = new StarboardEntry
            {
                MessageId = message.Id,
                BotMessageId = botMessage.Id,
                AuthorId = message.Author.Id,
                ChannelId = message.Channel.Id,
                StarboardChannelId = config.StarboardChannelId,
                StarCount = starCount,
                GuildId = guildChannel.Guild.Id
            };

            await _db.AddStarboardEntry(entry);
        }

        public async Task UpdateStarboardEntry(IUserMessage message, ISocketMessageChannel starboardChannel,
            StarboardEntry entry, SocketReaction reaction, int reactionThreshold)
        {
            int starCount = message.Reactions[reaction.Emote].ReactionCount;

            entry.StarCount = starCount;
            IUserMessage botMessage =
                await starboardChannel.GetMessageAsync(entry.BotMessageId) as IUserMessage;

            EmbedBuilder builder = botMessage.Embeds.First().ToEmbedBuilder();
            builder.Color = GetStarColor(starCount);

            var newBotmessage = $"{GetStarLevel(starCount, reactionThreshold)} " +
                $"**{starCount}** <#{entry.ChannelId}> [ID: {message.Id}]";

            await botMessage.ModifyAsync(msg =>
            {
                msg.Content = newBotmessage;
                msg.Embed = builder.Build();
            });

            await _db.UpdateStarboardEntry(entry);
        }

        private string GetStarLevel(int reactionCount, int threshold)
        {
            if (reactionCount == (threshold + 5))
                return starEmotes[0];
            else if (reactionCount == (threshold + 10))
                return starEmotes[1];
            else if (reactionCount == (threshold + 18))
                return starEmotes[2];
            else if (reactionCount == (threshold + 26))
                return starEmotes[3];
            else if (reactionCount == (threshold + 36))
                return starEmotes[5];
            else
                return starEmotes[0];
        }

        private Color GetStarColor(int reactionCount)
        {
            double p = reactionCount / 13;
            if (p > 1)
                p = 1;
            int green = (int)((194 * p) + (253 * (1 - p)));
            int blue = (int)((12 * p) + (247 * (1 - p)));

            return new Color(255, green, blue);
        }
    }
}
