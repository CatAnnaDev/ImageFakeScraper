using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace GScraper.Bing
{

    public class BingScraper
    {

        public BingScraper()
        {

        }

        private List<string> tmp = new();
        private const string uri = "https://www.bing.com/images/search?q={0}&ghsh=0&ghacc=0&first=1&tsc=ImageHoverTitle&adlt=off";
        private readonly Regex RegexCheck = new(@"^(http|https:):?([^\s([<,>]*)(\/)[^\s[,><]*(\?[^\s[,><]*)?");

        // (https:\/\/)?s?:?([^\s(["<,>/]*)(\/)[^\s[",><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s[",><]*)?

        // Regex.Match(url, @"^(?:https?://)?(?:[^@/\n]+@)?(?:www.)?([^:/?\n]+)").Groups[1].Value


        /// <summary>
        /// GetImagesAsync for Bing
        /// </summary>
        /// <param name="query">string param</param>
        /// <returns></returns>
        public async Task<List<string>> GetImagesAsync(string query)
        {
            try
            {
                tmp.Clear();
                GScraperGuards.NotNull(query, nameof(query));
                string[] args = new string[] { query};
                var doc = await httpRequest.Get(uri, args);
                IEnumerable<string> urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("src", null)).Where(s => !String.IsNullOrEmpty(s));

                //HtmlNodeCollection tag = doc.DocumentNode.SelectNodes("//div[@class='suggestion-title-wrapper']");
                //
                //if (tag != null)
                //{
                //    foreach (HtmlNode? item in tag)
                //    {
                //        if (addNewTag_Bing_Google || addNewTag_Bing)
                //        {
                //            if (await Read(redisConnection.GetDatabase, item.FirstChild.InnerText) == -1)
                //            {
                //                if (!qword.Contains(item.FirstChild.InnerText))
                //                    qword.Enqueue(item.FirstChild.InnerText);
                //            }
                //        }
                //    }
                //}

                foreach (string? data in urls)
                {
                    if (RegexCheck.IsMatch(data))
                    {
                        string cleanUrl = Regex.Replace(data, @"[?&][^?&]+=[^?&]+", "");
                        if (!cleanUrl.EndsWith("th") && !data.Contains("th?id="))
                            tmp.Add(cleanUrl);
                    }
                }           
            }
            catch (Exception e) { Console.WriteLine(e); }
            return tmp;
        }
    }
}