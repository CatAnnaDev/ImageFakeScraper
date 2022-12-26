using GScraper;
using GScraper.Brave;
using GScraper.DuckDuckGo;
using GScraper.Google;
using HtmlAgilityPack;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GScraperExample.function
{
    internal class searchEngineRequest
    {
        static GoogleScraper scraper = new GoogleScraper();
        static DuckDuckGoScraper duck = new DuckDuckGoScraper();
        static BraveScraper brave = new BraveScraper();

        static bool ddc = false;
        static bool brv = false;
        static bool printLog = false;
        static Dictionary<string, IEnumerable<IImageResult>> tmp = new();

        static Queue<string> qword = new();
        static HtmlNodeCollection table;

        public static async Task<Dictionary<string, IEnumerable<IImageResult>>> getAllDataFromsearchEngineAsync(string text)
        {


            if (GoogleScraper.gg)
            {
                IEnumerable<IImageResult> google;
                try
                {
                    google = await scraper.GetImagesAsync(text);
                    tmp.Add("Google", google);
                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Google: {e.Message}");
                    Console.ResetColor();
                    if (e.Message.Contains("429"))
                        GoogleScraper.gg = false;

                }
            }

            if (ddc)
            {
                IEnumerable<IImageResult> duckduck;
                try
                {
                    duckduck = await duck.GetImagesAsync(text);
                    tmp.Add("DuckDuckGo", duckduck);

                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Duckduckgo: {e.Message}");
                    Console.ResetColor();
                    if (e.Message.Contains("token") || e.Message.Contains("403"))
                        ddc = false;
                }
            }

            if (brv)
            {
                IEnumerable<IImageResult> bravelist;
                try
                {
                    bravelist = await brave.GetImagesAsync(text);
                    tmp.Add("Brave", bravelist);
                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Brave: {e.Message}");
                    Console.ResetColor();
                    if (e.Message.Contains("429"))
                        brv = false;

                }
            }

            if (!GoogleScraper.gg && !ddc && !brv)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("All search engine down for now");
                Console.ResetColor();
            }
            else if (GoogleScraper.gg && ddc && brv)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                await Console.Out.WriteLineAsync("All search engine up");
                Console.ResetColor();
            }
            else
            {
                if (!GoogleScraper.gg)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (printLog)
                        Console.WriteLine("Google stopped");
                    Console.ResetColor();
                    GoogleScraper.gg = true;
                }
                if (!ddc)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (printLog)
                        Console.WriteLine("Duckduckgo stopped");
                    Console.ResetColor();
                    //ddc = true;
                }
                if (!brv)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (printLog)
                        Console.WriteLine("Brave stopped");
                    Console.ResetColor();
                    //brv = true;
                }
            }

            return tmp;
        }

        public static async Task<Queue<string>> getAllNextTag(string text, ConnectionMultiplexer redis)
        {
            var region = new[] { "en", "fr" };
            Random rng = new();
            var choice = rng.Next(0, region.Length);
            var url = $"https://www.google.com/search?q={text}&tbm=isch&hl={region[choice]}";
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        string result = content.ReadAsStringAsync().Result;
                        HtmlDocument document = new();
                        document.LoadHtml(result);

                        table = document.DocumentNode.SelectNodes("//a[@class='TwVfHd']");

                        try
                        {
                            if (table != null)
                            {
                                for (var j = 0; j < table.Count; j++)
                                {
                                    try
                                    {
                                        if (await Read(redis, table[j].InnerText) == -1)
                                        {
                                            qword.Enqueue(table[j].InnerText);
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            await Console.Out.WriteLineAsync($"Tag Added {table[j].InnerText}");
                                            Console.ResetColor();
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            await Console.Out.WriteLineAsync($"Tag already exist {table[j].InnerText}");
                                            Console.ResetColor();
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            await Console.Out.WriteLineAsync("No Tag!");
                            await Console.Out.WriteLineAsync(e.Message);
                            Console.ResetColor();
                        }
                    }
                }
            }

            return qword;
        }

        private static async Task<long> Read(ConnectionMultiplexer redis, string text) => await redis.GetDatabase().ListPositionAsync("already_done_list", text);

    }
}
