using GScraper;
using GScraper.Brave;
using GScraper.DuckDuckGo;
using GScraper.Google;
using HtmlAgilityPack;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GScraperExample.function
{
    internal class searchEngineRequest
    {

        private static readonly GoogleScraper scraper = new();
        private static readonly DuckDuckGoScraper duck = new();
        private static readonly BraveScraper brave = new();
        private static bool ddc = false;
        private static bool brv = false;
        private static bool ov = true;
        private static bool bing = true;
        private static bool yahoo = true;
        private static readonly bool printLog = false;
        private static readonly Dictionary<string, IEnumerable<IImageResult>> tmp = new();
        private static readonly Queue<string> qword = new();
        private static HtmlNodeCollection? table;
        private static readonly List<NeewItem> OpenVersNewItem = new();
        private static readonly List<NeewItem> bingNewItem = new();
        private static readonly List<NeewItem> YahooNewItem = new();
        private static readonly HttpClient http = new();
        private static readonly Regex RegexCheck = new(@".*\.(jpg|png|gif)?$");
        private static DateTime? Openserv409;


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
                    {
                        GoogleScraper.gg = false;
                    }
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
                    {
                        ddc = false;
                    }
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
                    {
                        brv = false;
                    }
                }
            }

            if (ov)
            {
                try
                {
                    OpenVersNewItem.Clear();
                    int page = 1;
                    HttpResponseMessage resp = await http.GetAsync($"https://api.openverse.engineering/v1/images/?format=json&q={text}&page={page}&mature=true");

                    var data = await resp.Content.ReadAsStringAsync();
                    Root jsonparse = JsonConvert.DeserializeObject<Root>(data);
                    if (jsonparse != null)
                    {
                        if (jsonparse?.results != null)
                        {
                            if (jsonparse?.results.Count != 0)
                            {
                                if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                                {
                                    Openserv409 = DateTime.Now + resp?.Headers?.RetryAfter?.Delta;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Openverse RetryAfter {resp?.Headers?.RetryAfter?.Delta}");
                                    Console.ResetColor();
                                    //ov = false;
                                }
                                else
                                {
                                    try
                                    {
                                        for (int i = 0; i < jsonparse.results.Count; i++)
                                        {
                                            if (RegexCheck.IsMatch(jsonparse.results[i].url))
                                            {

                                                NeewItem blap2 = new()
                                                {
                                                    Url = jsonparse.results[i].url,
                                                    Title = jsonparse.results[i].title,
                                                    Height = 0,
                                                    Width = 0
                                                };

                                                OpenVersNewItem.Add(blap2);
                                            }
                                        }
                                        tmp.Add($"Openverse", OpenVersNewItem.AsEnumerable());

                                    }
                                    catch { }

                                }
                            }
                        }
                    }
                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Openverse: {e.Message}");
                    Console.ResetColor();
                    if (e.Message.Contains("429"))
                    {
                        ov = false;
                    }
                }
            }

            if (bing)
            {
                try
                {
                    bingNewItem.Clear();
                    var uri = $"https://www.bing.com/images/search?q={text}&ghsh=0&ghacc=0&first=1&tsc=ImageHoverTitle&cw=1224&ch=1215";
                    using HttpClient http = new HttpClient();

                    HttpResponseMessage resp = await http.GetAsync(uri);
                    var data = await resp.Content.ReadAsStringAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(data);
                    var urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("src2", null)).Where(s => !String.IsNullOrEmpty(s));

                    foreach (var datsa in urls)
                    {

                        NeewItem blap2 = new()
                        {
                            Url = datsa,
                            Title = "",
                            Height = 0,
                            Width = 0
                        };

                        bingNewItem.Add(blap2);
                    }
                    tmp.Add($"Bing", bingNewItem.AsEnumerable());

                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Bing: {e.Message}");
                    Console.ResetColor();
                    if (e.Message.Contains("429"))
                    {
                        bing = false;
                    }
                }
            }

            if (yahoo)
            {
                try
                {
                    YahooNewItem.Clear();
                    var uri = $"https://images.search.yahoo.com/search/images?ei=UTF-8&p={text}&fr2=p%3As%2Cv%3Ai&.bcrumb=4N2SA8f4BZT&save=0";
                    using HttpClient http = new HttpClient();

                    HttpResponseMessage resp = await http.GetAsync(uri);
                    var data = await resp.Content.ReadAsStringAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(data);
                    var urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("data-src", null)).Where(s => !String.IsNullOrEmpty(s));

                    foreach (var datsa in urls)
                    {

                        NeewItem blap2 = new()
                        {
                            Url = datsa.Replace("&pid=Api&P=0&w=300&h=300", ""),
                            Title = "",
                            Height = 0,
                            Width = 0
                        };

                        YahooNewItem.Add(blap2);
                    }
                    tmp.Add($"Yahoo", YahooNewItem.AsEnumerable());

                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Yahoo: {e.Message}");
                    Console.ResetColor();
                    if (e.Message.Contains("429"))
                    {
                        yahoo = false;
                    }
                }
            }

            if (!GoogleScraper.gg && !ddc && !brv && !ov && !bing && !yahoo)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("All search engine down for now");
                Console.ResetColor();
            }
            else if (GoogleScraper.gg && ddc && brv && ov && bing && yahoo)
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
                    {
                        Console.WriteLine("Google stopped");
                    }
                    Console.ResetColor();
                    GoogleScraper.gg = true;
                }
                if (!ddc)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (printLog)
                    {
                        Console.WriteLine("Duckduckgo stopped");
                    }
                    Console.ResetColor();
                    //ddc = true;
                }
                if (!brv)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (printLog)
                    {
                        Console.WriteLine("Brave stopped");
                    }
                    Console.ResetColor();
                    //brv = true;
                }
                if (!ov)
                {
                    // && DateTime.Now >= Openserv409
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (printLog)
                    {
                        Console.WriteLine("Openverse stopped");
                    }
                    Console.ResetColor();
                    ov = true;
                }
                if (!bing)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (printLog)
                    {
                        Console.WriteLine("Bing stopped");
                    }
                    Console.ResetColor();
                    bing = true;
                }
                if (!yahoo)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (printLog)
                    {
                        Console.WriteLine("Yahoo stopped");
                    }
                    Console.ResetColor();
                    yahoo = true;
                }
            }

            return tmp;
        }

        public static async Task<Queue<string>> getAllNextTag(string text, ConnectionMultiplexer redis)
        {

            string url = $"https://www.google.com/search?q={text}&tbm=isch&hl=en";
            using (HttpClient client = new())
            {
                try
                {
                    using HttpResponseMessage response = client.GetAsync(url).Result;
                    using HttpContent content = response.Content;
                    string result = content.ReadAsStringAsync().Result;
                    HtmlDocument document = new();
                    document.LoadHtml(result);

                    table = document.DocumentNode.SelectNodes("//a[@class='TwVfHd']");
                }
                catch { }

                try
                {
                    if (table != null)
                    {
                        for (int j = 0; j < table.Count; j++)
                        {
                            if (await Read(redis, table[j].InnerText) == -1)
                            {
                                //qword.Enqueue(table[j].InnerText);

                                //Console.ForegroundColor = ConsoleColor.Green;
                                //await Console.Out.WriteLineAsync($"Tag Added {table[j].InnerText}");
                                //Console.ResetColor();
                            }
                            else
                            {
                                //Console.ForegroundColor = ConsoleColor.Red;
                                //await Console.Out.WriteLineAsync($"Tag already exist {table[j].InnerText}");
                                //Console.ResetColor();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    await Console.Out.WriteLineAsync("No tag found!");
                    await Console.Out.WriteLineAsync(e.Message);
                    Console.ResetColor();
                }
            }

            return qword;
        }

        private static async Task<long> Read(ConnectionMultiplexer redis, string text)
        {
            return await redis.GetDatabase().ListPositionAsync("words_done", text);
        }
    }

    public class NeewItem : IImageResult
    {
        public string? Url { get; set; }

        public string? Title { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
