using Discord;
using Discord.Commands;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Web;
using TamamoSharp.Utils;

namespace TamamoSharp.Modules
{
    [Name("WebSearch")]
    [Summary("Various commands for searching the web and retrieving things.")]
    public class WebSearchModule : TamamoModuleBase
    {
        public readonly string GoogleApiKey;

        public WebSearchModule(IConfiguration cfg)
        {
            GoogleApiKey = cfg["tokens:google"];
        }

        [Command("ud"), Name("UrbanDictionary"), Alias("urbandictionary")]
        [Summary("Queries UrbanDictionary for the definition of a term.")]
        public async Task UrbanDictionary(string term, int termNum = 1)
        {
            termNum -= 1;

            string url = $"https://api.urbandictionary.com/v0/define?term={HttpUtility.HtmlEncode(term)}";
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

            EmbedBuilder embed = (new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = term.ToUpper(),
                    Url = (string)termData["permalink"],
                    IconUrl = embedIcon
                },
                Description = description,
                Color = new Color(52, 159, 244)
            });

            await ReplyAsync("", embed: embed.Build());
        }

        [Command("xkcd"), Name("XKCD")]
        [Summary("Shows a random comic from xkcd.")]
        public async Task XkcdComic()
        {
            string url = "https://c.xkcd.com/random/comic/";
            HtmlDocument doc = await WebHelpers.GetHtmlDocumentResponseAsync(url);
            string image = doc.DocumentNode.SelectSingleNode("//div[@id='comic']/img")
                .Attributes["src"].Value;
            await ReplyAsync($"https:{image}");
        }

        [Command("randomdog"), Name("RandomDog"), Alias("woof")]
        [Summary("Shows a random picture from random.dog.")]
        public async Task RandomDog()
            => await ReplyAsync($"https://random.dog/{await WebHelpers.GetResponseAsync("https://random.dog/woof")}");

        [Command("randomcat"), Name("RandomCat"), Alias("meow")]
        [Summary("Shows a random picture from random.cat.")]
        public async Task RandomCat()
            => await ReplyAsync((string)(await WebHelpers.GetJsonResponseAsync("https://random.cat/meow"))["file"]);

        [Command("shorten"), Name("Shorten")]
        [Summary("Uses goo.gl to shorten the given url.")]
        public async Task ShortenUrl(string url)
            => await ReplyAsync($"<{await WebHelpers.ShortenUrlAsync(GoogleApiKey, url)}>");
    }
}
