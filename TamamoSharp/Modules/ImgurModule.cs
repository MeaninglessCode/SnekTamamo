using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using TamamoSharp.Utils;

namespace TamamoSharp.Modules
{
    [Group("imgur"), Name("Imgur")]
    [Summary("A few commands for searching imgur.")]
    public class Imgur : TamamoModuleBase
    {
        private readonly string ImgurClientId;
        private readonly Random _rng;

        public Imgur(IConfiguration cfg, Random rng)
        {
            ImgurClientId = cfg["tokens:imgur_client_id"];
            _rng = rng;
        }

        [Command]
        [Priority(0)]
        public async Task RandomImage()
        {
            await ReplyAsync("No subcommand invoked! (will update later)");
        }

        [Command("search"), Name("Search")]
        [Summary("Searches imgur for an image based on a query.")]
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

            url += $"?q={HttpUtility.HtmlEncode(query)}";

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

            string links = string.Join('\n', (from result in response["data"] select (string)result["link"])
                .ToList().Take(results));
            await ReplyAsync(links);
        }

        [Command("subreddit"), Name("Subreddit")]
        [Summary("Searches imgur for an image from the given subreddit.")]
        [Priority(10)]
        public async Task Subreddit(string subreddit, string sort = "", string time = "")
        {
            string[] validSorts = { "new", "top" };
            string[] validTimes = { "day", "week", "month", "year", "all" };

            if (sort != "" && !validSorts.Contains(sort))
            {
                await DelayDeleteReplyAsync("Invalid sort method specified!", 5);
                return;
            }
            if (time != "" && !validTimes.Contains(time))
            {
                await DelayDeleteReplyAsync("Invalid time window specified!", 5);
                return;
            }

            string url = $"https://api.imgur.com/3/gallery/r/{HttpUtility.HtmlEncode(subreddit)}";

            if (sort != "") url += $"/{sort}";
            if (time != "") url += $"/{time}";

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

            await ReplyAsync((string)response["data"][_rng.Next(0, response["data"].Count())]["link"]);
        }
    }
}
