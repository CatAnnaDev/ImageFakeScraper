using HtmlAgilityPack;
using Newtonsoft.Json;
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
        private const string uri = "https://www.everypixel.com/search/search?q={0}&limit=9999&json=1&page={1}";
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


                for (int i = 1; i < 500; i++)
                {
                    string[] args = new string[] { query, i.ToString() };
                    var jsonGet = await httpRequest.GetJson(uri, args);
                    Root jsonparsed = JsonConvert.DeserializeObject<Root>(jsonGet);
                    if (jsonparsed != null)
                    {
                        if (jsonparsed.images != null)
                        {
                            if (jsonparsed.images.images_0 != null)
                            {
                                for (int j = 0; j < jsonparsed.images.images_0.Count; j++)
                                {
                                    if (RegexCheck.IsMatch(jsonparsed.images.images_0[i].url))
                                    {
                                        tmp.Add(jsonparsed.images.images_0[j].url);
                                    }
                                }
                            }
                        }
                        else
                            break;
                    }
                    else
                        break;
                    NbOfRequest++;
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
            return tmp;
        }
    }
}