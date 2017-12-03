using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TamamoSharp.Utils;

namespace TamamoSharp.Modules
{
    public class WebSearchModule : TamamoModuleBase
    {
        public readonly string GoogleApiKey;

        public WebSearchModule(IConfiguration cfg)
        {
            GoogleApiKey = cfg["tokens:google"];
        }

        [Command("ud"), Alias("urbandictionary")]
        public async Task UrbanDictionary(string term, int termNum = 1)
        {
            termNum -= 1;

            string url = $"https://api.urbandictionary.com/v0/define?term={term}";
            JObject response = await WebHelpers.GetJsonResponseAsync(url);

            if (response == null)
                return;

            int termCount = ((JArray)response["list"]).Count;
            if ((string)response["result_type"] == "no_results")
            {
                await DelayDeleteReplyAsync("No terms found!", 3);
                return;
            }
            else if (termNum > termCount - 1)
            {
                await DelayDeleteReplyAsync($"Invalid index! There are only {termNum} terms!");
                return;
            }

            JToken termData = response["list"][termNum];
            string description = $"**Author**: {termData["author"]}\n**Term**: {term.ToLower()}"
                + $" (+{termData["thumbs_up"]} -{termData["thumbs_down"]}) ({termCount} definitions)"
                + $"\n\n**Definition**: {termData["definition"]}\n\n**Example**: {termData["example"]}";
            string embedIcon = "https://goo.gl/SnxyHE";

            Embed embed = (new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = term.ToUpper(),
                    Url = (string)termData["permalink"],
                    IconUrl = embedIcon
                },
                Description = description,
                Color = new Color(52, 159, 244)
            })
            .Build();

            await ReplyAsync("", false, embed);
        }

        [Group("imgur")]
        public class Imgur : TamamoModuleBase
        {
            private readonly string ImgurClientId;

            public Imgur(IConfiguration cfg) {
                ImgurClientId = cfg["tokens:imgur_client_id"];
            }

            [Command]
            [Priority(0)]
            public async Task RandomImage()
            {
                await ReplyAsync("No subcommand invoked! (will update later)");
            }

            [Command("search")]
            [Priority(10)]
            public async Task Search(string query, int results = 1, string sort = "",
                string time = "", int page = 1)
            {
                string[] validSorts = { "time", "viral", "top" };
                string[] validTimes = { "day", "week", "month", "year", "all" };

                if (results > 3)
                {
                    await ReplyAsync("Too many results requested!");
                    return;
                }
                if ((sort != "") && !validSorts.Contains(sort))
                {
                    await ReplyAsync("Invalid sort specified!");
                    return;
                }
                if ((time != "") && !validTimes.Contains(time))
                {
                    await ReplyAsync("Invalid time window specified!");
                    return;
                }
                
                string url = "https://api.imgur.com/3/gallery/search/";

                if (sort != "") url += $"{sort}/";
                if (time != "") url += $"{time}/";
                if (sort != "" && time != "") url += $"{page.ToString()}/";

                url += $"?q={query}";

                WebHeaderCollection headers = new WebHeaderCollection
                {
                    ["Authorization"] = $"Client-ID {ImgurClientId}"
                };
                
                JObject response = await WebHelpers.GetJsonResponseAsync(url, headers);

                if ((string)response["success"] == "false")
                {
                    await DelayDeleteReplyAsync("No results found!", 5);
                    return;
                }

                int dataCount = ((JArray)response["data"]).Count();

                string links = string.Join('\n', (from result in response["data"] select (string)result["link"])
                    .ToList().Take(results));

                await ReplyAsync(links);
            }

            [Command]
            [Priority(10)]
            public async Task Subreddit()
            {

            }
        }
    }
}
