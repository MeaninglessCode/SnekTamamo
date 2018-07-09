using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TamamoSharp.Database;

namespace TamamoSharp.Modules
{
    [Group("star"), Name("Starboard")]
    [Summary("Various commands for help with other commands and modules.")]
    public class StarboardModule : TamamoModuleBase
    {
        [Command("enable"), Name("Enable")]
        [RequireOwner]
        public async Task EnableStarboard()
        {
            GuildConfig config = await Database.GetGuildConfigAsync(Context.Guild.Id);
            config.StarboardEnabled = true;

            await Database.UpdateGuildConfigAsync(config);
            await DelayDeleteReplyAsync("👍", 3);
        }

        [Command("setch"), Name("SetChannel")]
        [RequireOwner]
        public async Task SetStarboardChannel(string id)
        {
            if (!ulong.TryParse(id, out ulong channelId))
            {
                await DelayDeleteReplyAsync("Invalid channel ID given!", 3);
                return;
            }

            IGuildChannel channel = Context.Guild.GetChannel(channelId);
            if (channel == null)
            {
                await DelayDeleteReplyAsync("Channel not found!", 3);
                return;
            }

            GuildConfig config = await Database.GetGuildConfigAsync(Context.Guild.Id);
            if (config.StarboardEnabled == false)
            {
                await DelayDeleteReplyAsync("Starboard not enabled! :(", 3);
                return;
            }

            config.StarboardChannelId = channelId;
            await Database.UpdateGuildConfigAsync(config);
            await DelayDeleteReplyAsync("👍", 3);
        }
    }
}
