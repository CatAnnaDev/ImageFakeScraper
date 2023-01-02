using HtmlAgilityPack;
using System;
using System.Drawing;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;

namespace GScraper
{
    public class httpRequest
    {
        private static HttpClient client = new();
        private static HtmlDocument doc = new();
        
        public static async Task<HtmlDocument> Get(string uri, params object[] query)
        {
            var url = string.Format(uri, query);
            HttpResponseMessage resp = await client.GetAsync(url);
            var data = await resp.Content.ReadAsStringAsync();
            doc.LoadHtml(data);
            return doc;
        }

        public static async Task<string> GetJson(string uri, params object[] query)
        {
            var url = string.Format(uri, query);
            HttpResponseMessage resp = await client.GetAsync(url);
            var data = await resp.Content.ReadAsStringAsync();
            return data;
        }
    }
}
