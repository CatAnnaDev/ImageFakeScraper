using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace GScraper.Yahoo
{

    public class YahooScraper
    {

        public YahooScraper()
        {

        }

        private List<string> tmp = new();
        private const string uri = "https://images.search.yahoo.com/search/images?ei=UTF-8&p={0}&fr2=p%3As%2Cv%3Ai&.bcrumb=4N2SA8f4BZT&save=0";
        private readonly Regex RegexCheck = new(@"^(http|https:):?([^\s([<,>]*)(\/)[^\s[,><]*(\?[^\s[,><]*)?");

        /// <summary>
        /// GetImagesAsync for Yahoo
        /// </summary>
        /// <param name="query">string param</param>
        /// <returns></returns>
        public async Task<List<string>> GetImagesAsync(string query)
        {
            try
            {
                tmp.Clear();
                GScraperGuards.NotNull(query, nameof(query));
                string[] args = new string[] { query };
                var doc = await httpRequest.Get(uri, args);
                IEnumerable<string> urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("data-src", null)).Where(s => !String.IsNullOrEmpty(s));

                foreach (string? data in urls)
                {
                    if (RegexCheck.IsMatch(data))
                    {
                        string cleanUrl = Regex.Replace(data, @"&pid=Api&P=0&w=300&h=300", "");
                        if (!data.EndsWith("th"))
                            tmp.Add(cleanUrl);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
            return tmp;
        }
    }
}