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
using System.Net;
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
        private static bool ddc = true;
        private static bool brv = true;
        private static bool ov = true;
        private static bool bing = true;
        private static bool yahoo = true;
        private static bool getty = true;
        private static bool every = true;
        private static readonly bool printLog = false;
        private static readonly Dictionary<string, IEnumerable<IImageResult>> tmp = new();
        private static readonly Queue<string> qword = new();
        private static HtmlNodeCollection? table;
        private static readonly List<NeewItem> OpenVersNewItem = new();
        private static readonly List<NeewItem> bingNewItem = new();
        private static readonly List<NeewItem> YahooNewItem = new();
        private static readonly List<NeewItem> GettyNewItem = new();
        private static readonly List<NeewItem> EveryNewItem = new();
        private static readonly HttpClient http = new();
        private static readonly Regex RegexCheck = new(@".*\.(jpg|png|gif)?$");
        private static DateTime? Openserv409;

        private static bool addNewTag_Bing_Google = false;
        private static bool addNewTag_Bing = false;
        private static bool addNewTag_Google = false;

        public static int NbOfRequest = 0;


        public static async Task<Dictionary<string, IEnumerable<IImageResult>>> getAllDataFromsearchEngineAsync(string text)
        {
            if (GoogleScraper.gg)
            {
                IEnumerable<IImageResult> google;
                try
                {
                    google = await scraper.GetImagesAsync(text);
                    tmp.Add("Google", google);
                    NbOfRequest++;
                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Google: {e.Message}");
                    Console.ResetColor();
                    if (e.Message.Contains("429"))
                    {
                        GoogleScraper.gg = false;
                        tmp.Add("Google", null);
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
                    NbOfRequest++;

                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    //Console.WriteLine($"Duckduckgo: {e.Message}");
                    Console.ResetColor();
                    if (e.Message.Contains("token") || e.Message.Contains("403"))
                    {
                        ddc = false;
                        tmp.Add("DuckDuckGo", null);
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
                    NbOfRequest++;
                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    //Console.WriteLine($"Brave: {e.Message}");
                    Console.ResetColor();
                    if (e.Message.Contains("429"))
                    {
                        brv = false;
                        tmp.Add("Brave", null);
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
                    NbOfRequest++;
                    var data = await resp.Content.ReadAsStringAsync();
                    if (data.StartsWith("{"))
                    {
                        Root jsonparse = JsonConvert.DeserializeObject<Root>(data);
                        if (jsonparse != null)
                        {
                            if (jsonparse?.results != null)
                            {
                                if (jsonparse?.results.Count != 0)
                                {
                                    try
                                    {
                                        if (jsonparse.results.Count > 1)
                                        {
                                            for (int j = 0; j < jsonparse.page_count; j++)
                                            {
                                                resp = await http.GetAsync($"https://api.openverse.engineering/v1/images/?format=json&q={text}&page={page}&mature=true");
                                                NbOfRequest++;
                                                data = await resp.Content.ReadAsStringAsync();
                                                if (data.StartsWith("{"))
                                                {
                                                    jsonparse = JsonConvert.DeserializeObject<Root>(data);
                                                    if (jsonparse != null)
                                                    {
                                                        if (jsonparse?.results != null)
                                                        {
                                                            if (jsonparse?.results.Count != 0)
                                                            {
                                                                page++;
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

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
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
                                        }
                                        tmp.Add($"Openverse", OpenVersNewItem.AsEnumerable());
                                    }
                                    catch
                                    {
                                        tmp.Add($"Openverse", null);
                                    }
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
                        tmp.Add($"Openverse", null);
                    }
                }
            }

            if (bing)
            {
                try
                {
                    bingNewItem.Clear();
                    var uri = $"https://www.bing.com/images/search?q={text}&ghsh=0&ghacc=0&first=1&tsc=ImageHoverTitle&ADLT=OFF";

                    using HttpClient http = new HttpClient();

                    HttpResponseMessage resp = await http.GetAsync(uri);

                    var data = await resp.Content.ReadAsStringAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(data);
                    var urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("src2", null)).Where(s => !String.IsNullOrEmpty(s));

                    var tag = doc.DocumentNode.SelectNodes("//div[@class='suggestion-title-wrapper']");

                    if (tag != null)
                    {
                        foreach (var item in tag)
                        {
                            if (addNewTag_Bing_Google || addNewTag_Bing)
                            {
                                if (await Read(Program.redis, item.FirstChild.InnerText) == -1)
                                {
                                    if(!qword.Contains(item.FirstChild.InnerText))
                                        qword.Enqueue(item.FirstChild.InnerText);
                                }
                            }
                        }
                    }

                    foreach (var datsa in urls)
                    {

                        NeewItem blap2 = new NeewItem()
                        {
                            Url = datsa,
                            Title = "",
                            Height = 0,
                            Width = 0
                        };

                        bingNewItem.Add(blap2);
                    }
                    tmp.Add($"Bing", bingNewItem.AsEnumerable());
                    NbOfRequest++;
                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Bing: {e.Message}");
                    Console.ResetColor();
                    if (e.Message.Contains("429"))
                    {
                        bing = false;
                        tmp.Add($"Bing", null);
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
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        switch (resp.StatusCode)
                        {
                            case HttpStatusCode.TooManyRequests:
                                Console.ForegroundColor = ConsoleColor.Red;
                                //Console.WriteLine($"Yahoo Error 429");
                                Console.ResetColor();
                                yahoo = false;
                                tmp.Add($"Yahoo", null);
                                break;
                            case HttpStatusCode.InternalServerError:
                                Console.ForegroundColor = ConsoleColor.Red;
                                //Console.WriteLine($"Yahoo Error 500");
                                Console.ResetColor();
                                yahoo = false;
                                tmp.Add($"Yahoo", null);
                                break;
                        }
                    }
                    else
                    {
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
                        NbOfRequest++;
                    }
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

            if (getty)
            {
                try
                {
                    GettyNewItem.Clear();
                    var uri = $"https://www.gettyimages.fr/photos/{text}?assettype=image&family=creative&sort=best&suppressfamilycorrection=true&phrase={text}&license=rf%2Crm";
                    using HttpClient http = new HttpClient();

                    HttpResponseMessage resp = await http.GetAsync(uri);
                    var data = await resp.Content.ReadAsStringAsync();

                    var doc = new HtmlDocument();
                    doc.LoadHtml(data);
                    var urls = doc.DocumentNode.Descendants("source").Select(e => e.GetAttributeValue("srcSet", null)).Where(s => !String.IsNullOrEmpty(s));

                    foreach (var datsa in urls)
                    {
                        NeewItem blap2 = new()
                        {
                            Url = datsa.Replace("&amp;", "&"),
                            Title = "",
                            Height = 0,
                            Width = 0
                        };
                        GettyNewItem.Add(blap2);
                    }


                    uri = $"https://www.gettyimages.fr/photos/{text}?assettype=image&family=editorial&phrase={text}&license=rf%2Crm";

                    resp = await http.GetAsync(uri);
                    data = await resp.Content.ReadAsStringAsync();

                    doc = new HtmlDocument();
                    doc.LoadHtml(data);
                    urls = doc.DocumentNode.Descendants("source").Select(e => e.GetAttributeValue("srcSet", null)).Where(s => !String.IsNullOrEmpty(s));

                    foreach (var datsa in urls)
                    {
                        NeewItem blap2 = new()
                        {
                            Url = datsa.Replace("&amp;", "&"),
                            Title = "",
                            Height = 0,
                            Width = 0
                        };

                        GettyNewItem.Add(blap2);
                    }

                    tmp.Add($"Getty", GettyNewItem.AsEnumerable());
                    NbOfRequest++;
                }
                catch { tmp.Add($"Getty", null); }
            }

            if (every)
            {
                try
                {
                    EveryNewItem.Clear();
                    var uri = $"https://www.everypixel.com/search?q={text}&stocks_type=free";
                    using HttpClient http = new HttpClient();

                    HttpResponseMessage resp = await http.GetAsync(uri);
                    var data = await resp.Content.ReadAsStringAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(data);
                    var urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("src", null)).Where(s => !String.IsNullOrEmpty(s));

                    foreach (var datsa in urls)
                    {
                        if (!datsa.Contains("adserver"))
                        {
                            NeewItem blap2 = new()
                            {
                                Url = datsa,
                                Title = "",
                                Height = 0,
                                Width = 0
                            };

                            EveryNewItem.Add(blap2);
                        }
                    }
                    tmp.Add($"Every", EveryNewItem.AsEnumerable());
                    NbOfRequest++;
                }
                catch { tmp.Add($"Every", null);  }
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
                                if (addNewTag_Bing_Google || addNewTag_Google)
                                    qword.Enqueue(table[j].InnerText);

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
                        NbOfRequest++;
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
