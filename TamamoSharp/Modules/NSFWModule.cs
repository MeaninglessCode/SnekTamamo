using Discord.Commands;
using HtmlAgilityPack;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using TamamoSharp.Utils;

namespace TamamoSharp.Modules
{
    [Name("NSFW")]
    [Summary("spooky commands")]
    public class NSFWModule : TamamoModuleBase
    {
        private readonly Random _rng;

        public NSFWModule(Random rng)
        {
            _rng = rng;
        }

        [Command("tbib")]
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

        [Command("danbooru")]
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
    }
}
