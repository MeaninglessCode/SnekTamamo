using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TamamoSharp.Utils
{
    public static class WebHelpers
    {
        public static async Task<string> ShortenUrlAsync(string apiKey, string url)
        {
            WebRequest req = WebRequest.Create(
                $"https://www.googleapis.com/urlshortener/v1/url?key={apiKey}");

            req.ContentType = "application/json";
            req.Method = "POST";

            using (StreamWriter writer = new StreamWriter(await req.GetRequestStreamAsync()))
                await writer.WriteAsync($"{{\"longUrl\":\"{url}\"}}");

            WebResponse res = await req.GetResponseAsync();
            return (string)JObject.Parse(await (new StreamReader(res.GetResponseStream()))
                .ReadToEndAsync())["id"];
        }

        public static async Task<JObject> GetJsonResponseAsync(string url, WebHeaderCollection headers = null)
        {
            WebRequest req = WebRequest.Create(url);
            if (headers != null)
                req.Headers = headers;

            WebResponse res = await req.GetResponseAsync();
            return JObject.Parse(await (new StreamReader(res.GetResponseStream())
                .ReadToEndAsync()));
        }

        public static async Task<string> GetResponseAsync(string url, WebHeaderCollection headers = null)
        {
            WebRequest req = WebRequest.Create(url);
            if (headers != null)
                req.Headers = headers;

            WebResponse res = await req.GetResponseAsync();
            return (await (new StreamReader(res.GetResponseStream())
                .ReadToEndAsync()));
        }

        public static async Task<XDocument> GetXmlResponseAsync(string url, WebHeaderCollection headers = null)
        {
            WebRequest req = WebRequest.Create(url);
            if (headers != null)
                req.Headers = headers;

            WebResponse res = await req.GetResponseAsync();
            return XDocument.Parse(await (new StreamReader(res.GetResponseStream()))
                .ReadToEndAsync());

        }

        public static async Task<HtmlDocument> GetHtmlDocumentResponseAsync(string url,
            WebHeaderCollection headers = null)
        {
            WebRequest req = WebRequest.Create(url);
            if (headers != null)
                req.Headers = headers;

            HtmlDocument doc = new HtmlDocument();
            WebResponse res = await req.GetResponseAsync();

            doc.LoadHtml(await (new StreamReader(res.GetResponseStream()))
                .ReadToEndAsync());

            return doc;
        }
    }
}
