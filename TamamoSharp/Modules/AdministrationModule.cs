using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TamamoSharp.Database;

namespace TamamoSharp.Modules
{
    [Name("Administration")]
    [Summary("Commands for performing administrative actions within a guild.")]
    public class AdministrationModule : TamamoModuleBase
    {
        [Command("setgame")]
        [RequireOwner]
        public async Task SetGame(string name)
            => await Context.Client.SetGameAsync(name);
        
        [Command("ginfo")]
        [RequireContext(ContextType.Guild)]
        public async Task GuildInfo(ulong guildId = 0)
        {
            SocketGuild guild = (guildId == 0)? Context.Guild : Context.Client.GetGuild(guildId);

            string roleList = string.Join(", ", from role in guild.Roles select role.Name);
            int onlineUsers = guild.Users.Where(x => x.Status == UserStatus.Online).Count();
            int offlineUsers = guild.Users.Where(x => x.Status == UserStatus.Offline).Count();
            int idleUsers = guild.Users.Where(x => x.Status == UserStatus.Idle).Count();

            EmbedBuilder builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = guild.IconUrl,
                    Name = guild.Name
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Created: {guild.CreatedAt}"
                },
                ThumbnailUrl = guild.IconUrl
            }
            .AddField(new EmbedFieldBuilder
            {
                Name = "Owner",
                Value = $"{guild.Owner.Username}#{guild.Owner.Discriminator}",
                IsInline = true
            })
            .AddField(new EmbedFieldBuilder
            {
                Name = "Region", Value = guild.VoiceRegionId, IsInline = true
            })
            .AddField(new EmbedFieldBuilder
            {
                Name = "Members",
                Value = $"{guild.MemberCount}\n(Online: {onlineUsers})\n(Offline: {offlineUsers})"
                    + $"(Idle: {idleUsers})",
                IsInline = true
            })
            .AddField(new EmbedFieldBuilder
            {
                Name = "Roles", Value = roleList, IsInline = false
            });

            await ReplyAsync("", embed: builder.Build());

        }

        [Group("prune"), Name("Prune")]
        [Summary("Several methods of pruning messages from a guild.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public class Prune : TamamoModuleBase
        {
            [Command, Name("PruneTamamo")]
            [Summary("Cleans up Tamamo's recent messages in the current channel.")]
            [Priority(0)]
            public async Task PruneSelf(int num = 100)
                => await DoPrune(Context, x => x.Author == Context.User, num);

            [Command("user"), Name("PruneUser")]
            [Summary("Prunes n recent messages from specified user in the current channel.")]
            [Priority(10)]
            public async Task PruneUser(SocketGuildUser user, int num = 100)
                => await DoPrune(Context, x => x.Author == user, num);

            [Command("all"), Name("PruneAll")]
            [Summary("Prunes n most recent messages from the current channel.")]
            [Priority(10)]
            public async Task PruneAll(int num = 100)
                => await DoPrune(Context, x => true, num);

            [Command("embeds"), Name("PruneEmbeds")]
            [Summary("Prunes n most recent messages containing embeds in the current channel.")]
            [Priority(10)]
            public async Task PruneEmbeds(int num = 100)
                => await DoPrune(Context, x => x.Embeds.Count > 0, num);

            [Command("files"), Name("PruneFiles")]
            [Summary("Prunes n most recent messages containing files in the current channel.")]
            [Priority(10)]
            public async Task PruneFiles(int num = 100)
                => await DoPrune(Context, x => x.Attachments.Count > 0, num);

            [Command("contains"), Name("PruneSubstring")]
            [Summary("Prunes n most recent messages containing the specified substring in the"
                + "current channel.")]
            [Priority(10)]
            public async Task PruneSubstring(string subString, int num = 100)
                => await DoPrune(Context, x => x.Content.Contains(subString), num);

            [Command("bot"), Name("PruneBot")]
            [Summary("Prunes n most recent messages sent by bot users in the current channel.")]
            [Priority(10)]
            public async Task PruneBot(int num = 100)
                => await DoPrune(Context, x => x.Author.IsBot, num);

            private async Task DoPrune(SocketCommandContext ctx, Func<IMessage, bool> expr, int count = 100)
            {
                ITextChannel channel = ctx.Channel as ITextChannel;
                IEnumerable<IMessage> toDelete = (await channel.GetMessagesAsync(count).FlattenAsync()).Where(expr);

                await channel.DeleteMessagesAsync(toDelete);

                int deleted = toDelete.Count();
                List<string> result = new List<string>
                {
                    $"{deleted} message{((deleted == 1) ? " was" : "s were")} pruned!"
                };

                if (deleted > 0)
                {
                    result.Add(" ");
                    Dictionary<string, int> authors = toDelete.GroupBy(x => $"{x.Author.Username}#{x.Author.Discriminator}")
                        .ToDictionary(x => x.Key, x => x.Count());

                    foreach (var i in authors)
                        result.Add($"{i.Key}: {i.Value}");
                }

                string toSend = string.Join("\n", result);
                if (toSend.Length > 2000)
                    await DelayDeleteReplyAsync($"Pruned {deleted} messages!", 5);
                else
                    await DelayDeleteReplyAsync(toSend, 5);
            }
        }

        [Group("ignore")]
        public class Ignore : TamamoModuleBase
        {
            /*
            [Command("user")]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.MuteMembers)]
            public async Task IgnoreUser(SocketGuildUser user)
            {
                if (await Database.UserIgnoredAsync(Context.Guild.Id, user.Id))
                {
                    await DelayDeleteReplyAsync("That user is already ignored!", 5);
                    return;
                }

                GuildConfig gc = await Database.GetGuildConfigAsync(Context.Guild.Id);
                IgnoredUser ignore = new IgnoredUser
                {
                    GuildId = Context.Guild.Id,
                    UserId = user.Id,
                    GuildConfig = gc
                };

                await Database.IgnoreUserAsync(ignore);
                await DelayDeleteReplyAsync($"User `{user.Username}#{user.Discriminator}` ignored!", 5);
            }
            */

            [Command("channel")]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.MuteMembers)]
            public async Task IgnoreChannel(SocketGuildChannel channel)
            {
                if (await Database.IsChannelIgnoredAsync(channel.Id))
                {
                    await DelayDeleteReplyAsync("That channel is already ignored!", 5);
                    return;
                }

                GuildConfig gc = await Database.GetGuildConfigAsync(Context.Guild.Id);
                ChannelConfig ignore = new ChannelConfig
                {
                    GuildId = Context.Guild.Id,
                    ChannelId = channel.Id
                };

                await Database.IgnoreChannelAsync(ignore);
                await ReplyAsync($"Channel `#{channel.Name}` ignored!");
            }

            [Command("guild")]
            [RequireOwner]
            public async Task IgnoreGuild(ulong guildId)
            {
                SocketGuild guild = Context.Client.GetGuild(guildId);
                if (guild == null)
                {
                    await DelayDeleteReplyAsync("Guild not found!", 5);
                    return;
                }

                await Database.IgnoreGuildAsync(guildId);
                await ReplyAsync($"Guild `{guild.Name}({guild.Id})` ignored!");
            }
        }

        [Group("unignore")]
        public class UnIgnore : TamamoModuleBase
        {
            /*
            [Command("user")]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.MuteMembers)]
            public async Task UnIgnoreUser(SocketGuildUser user)
            {
                IgnoredUser u = await _gcdb.GetIgnoredUserAsync(Context.Guild.Id, user.Id);
                if (u == null)
                {
                    await DelayDeleteReplyAsync("That user is not ignored!", 5);
                    return;
                }

                await _gcdb.UnIgnoreUserAsync(u);
                await ReplyAsync($"User `{user.Username}#{user.Discriminator}` unignored!");
            }
            */

            [Command("channel")]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.MuteMembers)]
            public async Task UnIgnoreChannel(SocketGuildChannel channel)
            {
                ChannelConfig config = await Database.GetChannelConfigAsync(channel.Id);
                if (config == null)
                {
                    await DelayDeleteReplyAsync("That channel is not ignored!", 5);
                    return;
                }

                await Database.UnIgnoreChannelAsync(config);
                await ReplyAsync($"Channel `#{channel.Name}` unignored!");
            }

            [Command("guild")]
            [RequireOwner]
            public async Task UnignoreGuild(ulong guildId)
            {
                SocketGuild guild = Context.Client.GetGuild(guildId);
                if (guild == null)
                {
                    await DelayDeleteReplyAsync("Guild not found!", 5);
                    return;
                }

                await Database.UnIgnoreGuildAsync(guildId);
                await ReplyAsync($"Guild `{guild.Name}({guild.Id})` unignored!");
            }
        }

        [Command("kick")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Kick(SocketGuildUser user, string reason = null)
        {
            await user.KickAsync(reason);
            await ReplyAsync($"{user.Username} has been kicked.");
        }

        [Command("softban")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task SoftBan(SocketGuildUser user, string reason = null)
        {
            await Context.Guild.AddBanAsync(user, reason: reason);
            await Context.Guild.RemoveBanAsync(user);
            await ReplyAsync(":ok_hand:");
        }
    }
}
