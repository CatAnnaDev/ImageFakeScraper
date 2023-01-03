using HtmlAgilityPack;
using System;
using System.Drawing;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

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

        public static async Task<string> PostJson(string uri, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await client.PostAsync(uri, content);
            var data = await result.Content.ReadAsStringAsync();
            return data;
        }
    }
}
