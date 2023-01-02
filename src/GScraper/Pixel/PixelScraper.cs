using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GScraper.Pixel
{
    public class PixelScraper
    {
        public PixelScraper()
        {
        }

        private List<string> tmp = new();
        public int NbOfRequest = 0;
        private const string uri = "https://www.everypixel.com/search?q={0}&page={1}";
        private readonly Regex RegexCheck = new(@"^(http|https:\/\/):?([^\s([<,>\/]*)(\/)[^\s[,><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s[,><]*)?");

        /// <summary>
        /// GetImagesAsync for Pixel
        /// </summary>
        /// <param name="query">string param</param>
        /// <returns></returns>
        public async Task<List<string>> GetImagesAsync(string query)
        {
            try
            {
                tmp.Clear();
                NbOfRequest = 0;
                GScraperGuards.NotNull(query, nameof(query));
                for (int i = 1; i < 100; i++)
                {
                    
                    object[] args = new object[] { query, i.ToString() };
                    var doc = await httpRequest.Get(uri, args);
                    IEnumerable<string> urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("src", null)).Where(s => !String.IsNullOrEmpty(s));

                    if (urls.Count() == 0)
                        break;

                    //if (urls.First().Contains("INSERT_RANDOM_NUMBER_HERE"))
                    //    break;

                    foreach (string? data in urls)
                    {
                        if (!data.Contains("adserver"))
                        {
                            if (RegexCheck.IsMatch(data))
                            {
                                tmp.Add(data);
                            }
                        }
                    }
                    NbOfRequest++;
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
            return tmp;
        }
    }
}