using Discord.Commands;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using TamamoSharp.Utils;

namespace TamamoSharp.Modules
{
    [Name("NSFW")]
    [Summary("Commands that are not safe for work.")]
    public class NSFWModule : TamamoModuleBase
    {
        private readonly Random _rng;

        public NSFWModule(Random rng)
        {
            _rng = rng;
        }

        [Command("tbib"), Name("TBIB")]
        [Summary("Searches TBIB for a random image based on the given tags.")]
        public async Task SearchTbib([Remainder] string tags = "")
        {
            string url = "http://tbib.org/index.php?page=dapi&s=post&q=index&limit=0";

            if (tags != "")
            {
                tags = tags.Replace(' ', '+');
                url = $"http://tbib.org/index.php?page=dapi&s=post&q=index&limit=0&tags={tags}";
            }

            XDocument result = await WebHelpers.GetXmlResponseAsync(url);
            int postCount = int.Parse(result.Root.Attribute("count").Value);

            if (postCount <= 0)
            {
                await DelayDeleteReplyAsync("One or more tags not found!", 5);
                return;
            }

            url = "http://tbib.org/index.php?page=dapi&s=post&q=index&limit=1&tags="
                + $"{tags}&pid={_rng.Next(0, postCount)}";
            result = await WebHelpers.GetXmlResponseAsync(url);

            if (result is null)
                return;

            await ReplyAsync(result.Root.Element("post").Attribute("file_url").Value);
        }

        [Command("rule34"), Name("Rule34")]
        [Summary("")]
        public async Task SearchRule34([Remainder] string tags = "")
        {
            string url = "https://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=0";

            if (!string.IsNullOrWhiteSpace(tags))
            {
                tags = tags.Replace(' ', '+');
                url = $"https://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=0&tags={tags}";
            }

            XDocument result = await WebHelpers.GetXmlResponseAsync(url);
            int postCount = int.Parse(result.Root.Attribute("count").Value);

            if (postCount <= 0)
            {
                await DelayDeleteReplyAsync("", 5);
                return;
            }

            url = $"https://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=1&tags="
                + $"{tags}&pid={_rng.Next(0, postCount)}";
            result = await WebHelpers.GetXmlResponseAsync(url);

            if (result is null)
                return;

            await ReplyAsync($"https:{result.Root.Element("post").Attribute("file_url").Value}");
        }

        [Command("danbooru"), Name("Danbooru")]
        [Summary("Searches Danbooru for a random image based on the given tags.")]
        public async Task SearchDanbooru([Remainder] string tags = "")
        {
            if (tags.Split(' ').Length > 2)
            {
                await DelayDeleteReplyAsync("No more than 2 tags at a time, please!", 5);
                return;
            }

            tags = tags.Replace(' ', '+');
            string url = $"http://danbooru.donmai.us/posts/random/?tags={tags}";
            HtmlDocument doc = await WebHelpers.GetHtmlDocumentResponseAsync(url);

            url = doc.GetElementbyId("image-container").GetAttributeValue("data-file-url", "");
            Console.WriteLine($"Url: {url}");

            if (url == "")
                await DelayDeleteReplyAsync("One or more tags not found!", 5);
            else
                await ReplyAsync($"http://danbooru.donmai.us{url}");
        }

        [Command("boobs"), Alias("showbobs")]
        public async Task SearchOBoobs()
        {
            string response = (await WebHelpers.GetResponseAsync("http://api.oboobs.ru/noise/1"))
                .Replace('[', ' ').Replace(']', ' ');
            JObject json = JObject.Parse(response);
            await ReplyAsync($"http://media.oboobs.ru/{json["preview"]}");
        }

        [Command("butt"), Alias("showvegene")]
        public async Task SearchOButts()
        {
            string response = (await WebHelpers.GetResponseAsync("http://api.obutts.ru/noise/1"))
                .Replace('[', ' ').Replace(']', ' ');
            JObject json = JObject.Parse(response);
            await ReplyAsync($"http://media.obutts.ru/{json["preview"]}");
        }
    }
}
