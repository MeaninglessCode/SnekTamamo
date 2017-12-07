using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TamamoSharp.Database.Tags;

namespace TamamoSharp.Modules
{
    [Group("tag"), Name("Tag")]
    [Summary("Commands for creating and managing tagged text.")]
    public class TagsModule : TamamoModuleBase
    {
        private readonly TagDb _tdb;

        public TagsModule(TagDb tdb)
        {
            _tdb = tdb;
        }

        [Command, Name("GetTag")]
        [Summary("By default, displays the tag with the given name if no subcommands"
            + "are invoked.")]
        [Priority(0)]
        public async Task GetTag(string name)
        {
            Tag tag = await _tdb.GetTagAsync(Context.Guild.Id, name);

            if (tag == null)
            {
                await DelayDeleteReplyAsync($"Tag **{name}** not found!", 5);
                return;
            }

            await _tdb.AddUseAsync(tag);
            await ReplyAsync($"**{tag.Name}**:\n{tag.Content}");
        }

        [Command("list"), Name("ListTags"), Alias("l")]
        [Summary("Lists all tags available to the current guild.")]
        [Priority(10)]
        public async Task ListTags()
        {
            Tag[] tags = await _tdb.GetTagsAsync(Context.Guild.Id);

            if (tags.Length <= 0)
                await DelayDeleteReplyAsync("No tags found for this guild!", 5);
            else
            {
                List<string> output = new List<string>();
                foreach (Tag t in tags)
                {
                    TagAlias[] aliases = await _tdb.GetAliasesAsync(t.Id);
                    StringBuilder sb = new StringBuilder()
                        .Append($"{t.Name}");
                    if (aliases.Length > 0)
                    {
                        sb.Append($" (");
                        sb.Append(string.Join(", ", from alias in aliases select alias.Name));
                        sb.Append(")");
                    }
                    output.Add(sb.ToString());
                }
                await ReplyAsync($"Tags:\n{string.Join(", ", output)}");
            }
        }

        [Command("add"), Name("AddTag"), Alias("a")]
        [Summary("Adds a new tag with the given name and content to the current guild.")]
        [Priority(10)]
        public async Task AddTag(string name, [Remainder] string content)
        {
            if (await _tdb.GetTagAsync(Context.Guild.Id, name) != null)
            {
                await DelayDeleteReplyAsync($"A tag with the name or alias **{name}** already exists!", 5);
                return;
            }

            await _tdb.AddTagAsync(new Tag
            {
                Name = name,
                Content = content,
                OwnerId = Context.Message.Author.Id,
                GuildId = Context.Guild.Id,
                Type = "guild"
            });

            await DelayDeleteReplyAsync("Tag successfully created!", 5);
        }

        [Command("remove"), Alias("r", "d", "delete")]
        [Summary("Removes an existing tag from the current guild.")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Priority(10)]
        public async Task RemoveTag(string name)
        {
            Tag tag = await _tdb.GetTagAsync(Context.Guild.Id, name);

            if (tag == null)
                await DelayDeleteReplyAsync($"Tag **{name}** not found!", 5);
            else
            {
                await _tdb.DeleteTagAsync(tag);
                await ReplyAsync("Tag deleted!");
            }
        }

        [Command("alias"), Name("Alias")]
        [Summary("Adds a new alias name to the specified tag in the current guild.")]
        [Priority(10)]
        public async Task AddAlias(string tagName, string aliasName)
        {
            Tag tag = await _tdb.GetTagAsync(Context.Guild.Id, tagName);
            if (tag == null)
            {
                await DelayDeleteReplyAsync($"Tag **{tagName}** not found!", 5);
                return;
            }

            await _tdb.AddAliasAsync(new TagAlias
            {
                TagId = tag.Id,
                Name = aliasName,
                OwnerId = Context.Message.Author.Id,
                GuildId = tag.GuildId,
                Tag = tag
            });

            await ReplyAsync($"**{tagName}** successfully aliased to **{aliasName}**!");
        }

        [Command("unalias"), Name("Unalias")]
        [Summary("Removes an alias name from the specified tag in the current guild.")]
        [Priority(10)]
        public async Task RemoveAlias(string tagName, string aliasName)
        {
            Tag tag = await _tdb.GetTagAsync(Context.Guild.Id, tagName);
            
            if (tag == null)
            {
                await DelayDeleteReplyAsync($"Tag **{tagName}** not found!", 5);
                return;
            }

            TagAlias alias = tag.Aliases.SingleOrDefault(x => x.Name.ToLower() == aliasName.ToLower());
            if (alias == null)
            {
                await DelayDeleteReplyAsync($"Tag **{tagName}** has no alias matching **{aliasName}**.", 5);
                return;
            }

            await _tdb.DeleteAliasAsync(alias);
            await ReplyAsync("Alias removed!");
        }

        [Command("info"), Name("TagInfo")]
        [Summary("Displays detailed information about the specified tag in the current guild.")]
        [Priority(10)]
        public async Task TagInfo(string name)
        {
            Tag tag = await _tdb.GetTagAsync(Context.Guild.Id, name);

            if (tag == null)
            {
                await DelayDeleteReplyAsync($"Tag **{name}** not found!", 5);
                return;
            }

            IUser author = await Context.Channel.GetUserAsync(tag.OwnerId);
            TagAlias[] aliases = await _tdb.GetAliasesAsync(tag.Id);
            string aliasNames = string.Join(", ", from alias in aliases select alias.Name);

            EmbedBuilder builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $"{author.Username}#{author.Discriminator}",
                    IconUrl = author.GetAvatarUrl()
                },
                Description = $"Name: {tag.Name}\nAliases: {aliasNames}\nUses: {tag.Uses}"
                + $"\nCreated: {tag.CreatedAt.ToString("ddd, MMM d, yyyy h:mm:ss tt")}"
                + $"\nLast Updated: {tag.UpdatedAt.ToString("ddd, MMM d, yyyy h:mm:ss tt")}"
            };

            await ReplyAsync("", embed: builder.Build());
        }
    }
}
