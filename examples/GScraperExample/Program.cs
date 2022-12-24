﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GScraper;
using GScraper.Brave;
using GScraper.DuckDuckGo;
using GScraper.Google;
using HtmlAgilityPack;
using StackExchange.Redis;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GScraperExample;

internal static class Program
{
    private static async Task Main(string[] args)
    {

        var options = ConfigurationOptions.Parse("imagefake.net:6379");
        options.Password = "yoloimage";
        options.ReconnectRetryPolicy = new ExponentialRetry(10);
        options.CommandMap = CommandMap.Create(new HashSet<string> { "SUBSCRIBE" }, false);
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);

        List<IEnumerable<IImageResult>> images = new();

        bool ddc = true;
        bool brv = true;

        bool printLog = true;

        IDatabase conn = redis.GetDatabase();

        using var scraper = new GoogleScraper();
        using var duck = new DuckDuckGoScraper();
        using var brave = new BraveScraper();
        HtmlNodeCollection table;
        string actual;

        bool stopBrave = false;

        List<string> qword = new();

        var key = new RedisKey("already_done_ZADD");
        var meow = await conn.SortedSetRangeByRankAsync(key, stop:1, order: Order.Descending);
        string text = meow.FirstOrDefault();
        qword.Add(text);

        int waittime;
        if (args.Length > 1)
            waittime = int.Parse(args[1]);
        else
            waittime = 0;

        //string[] readText = File.ReadAllLines("google_twunter_lol.txt");
        //foreach (string s in readText)
        //{
        //    qword.Add(s);
        //}
        //
        //qword.Reverse();


        if (redis.IsConnected)
        {

            // ImageDownloader.DownloadImagesFromUrl("https://techno.firenode.net/index.sh");

            //Thread thread = new Thread(() => Reddit.RedditCrawler(redis));
            //thread.Start();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Out.WriteLineAsync("Redis Connected");
            Console.ResetColor();

            Console.Out.WriteLineAsync("=====================================================================");
            Console.Out.WriteLineAsync(qword[0]);
            Console.Out.WriteLineAsync("=====================================================================");

            for (int i = 0; i < qword.Count; i++)
            {

                if (GoogleScraper.gg)
                {
                    IEnumerable<IImageResult> google;
                    try
                    {
                        google = await scraper.GetImagesAsync(text);
                        images.Add(google);
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
                        images.Add(duckduck);

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
                        images.Add(bravelist);
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
                    Console.Out.WriteLineAsync("All search engine down for now");
                    Console.ResetColor();
                    break;
                }
                else if (GoogleScraper.gg && ddc && brv)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Out.WriteLineAsync("All search engine up");
                    Console.ResetColor();
                }
                else
                {
                    if (!GoogleScraper.gg)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Google stopped");
                        Console.ResetColor();
                        GoogleScraper.gg = true;
                    }
                    if (!ddc)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Duckduckgo stopped");
                        Console.ResetColor();
                        ddc = true;
                    }
                    if (!brv)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Brave stopped");
                        Console.ResetColor();
                        brv = true;
                    }
                }

                var url = $"https://www.google.com/search?q={text}&tbm=isch&hl=en";
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
                                            if (!await Read(redis, table[j].InnerText))
                                            {
                                                qword.Add(table[j].InnerText);
                                            }
                                            else
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.Out.WriteLineAsync($"Tag already exist {table[j].InnerText}");
                                                Console.ResetColor();
                                            }
                                        }
                                        catch { }
                                    }
                                    var listduplicate = RemoveDuplicatesSet(qword);
                                    qword.Clear();
                                    qword.AddRange(listduplicate);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Out.WriteLineAsync("No Tag!");
                                Console.Out.WriteLineAsync(e.Message);
                                Console.ResetColor();
                            }
                        }
                    }
                }

                if (table == null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Out.WriteLineAsync("No more Tag found!");
                    Console.ResetColor();
                }

                foreach (var image in images)
                {
                    if (image != null)
                    {

                        var list = new List<string>();

                        foreach (var daata in image)
                        {
                            if (printLog)
                            {
                                Console.WriteLine();
                                Console.WriteLine(JsonSerializer.Serialize(daata, daata.GetType(), new JsonSerializerOptions { WriteIndented = true }));
                                Console.WriteLine(daata.ToString());
                            }
                            list.Add(daata.Url);

                            if (printLog)
                                Console.WriteLine();
                        }

                        var parse = list.ToArray();
                        var push = Array.ConvertAll(parse, item => (RedisValue)item);
                        try
                        {
                            await conn.SetAddAsync("image_jobs", push);
                        }
                        catch { Console.ForegroundColor = ConsoleColor.Red; Console.Out.WriteLineAsync("Fail upload redis !"); Console.ResetColor(); }

                        list.Clear();
                    }
                    else
                    {
                        Console.Out.WriteLineAsync("Image is null fix it yourself !");
                    }
                }
                images.Clear();
                text = qword[i];

                if (!redis.IsConnected)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Out.WriteLineAsync("redis disconnected, press enter to stop");
                    Console.ResetColor();
                    Console.ReadLine();
                    break;
                }

                if (conn.SetLength("image_jobs") == uint.MaxValue - 10000)
                {
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Out.WriteLineAsync($"Redis queue alomst full {conn.ListLength("image_jobs")}");
                        Console.ResetColor();
                        Console.ReadLine();
                    }
                    catch { }
                }

                try
                {
                    write(text, redis);
                }catch { }

                Console.Out.WriteLineAsync("================================================================================================================================");
                try
                {
                    Console.Out.WriteLineAsync($"Previous done: {text}, Next: {qword[i + 1]}, Tag in queue: {qword.Count}, Redis ListLen: {conn.SetLength("image_jobs")}({(100.0 * (float)conn.SetLength("image_jobs") / (float)uint.MaxValue).ToString("0.000")}%) / {uint.MaxValue}, already done word: {await redis.GetDatabase().SortedSetLengthAsync(key)}");
                }
                catch { }
                Console.Out.WriteLineAsync("================================================================================================================================");
                Console.Out.WriteLineAsync($"Sleep {waittime}sec;");
                Thread.Sleep(TimeSpan.FromSeconds(waittime));
            }
        }
    }

    private static async void write(string text, ConnectionMultiplexer redis)
    {
        var value = new RedisValue(text);
        var key = new RedisKey("already_done_ZADD");
        await redis.GetDatabase().SortedSetAddAsync(key, value, await redis.GetDatabase().SortedSetLengthAsync(key) + 1);
    }

    private static async Task<bool> Read(ConnectionMultiplexer redis, string text) => await redis.GetDatabase().SetContainsAsync("already_done_list", text);

    public static List<string> RemoveDuplicatesSet(List<string> items)
    {
        if (items.Count == 1)
            return items;

        var result = new List<string>();
        var set = new HashSet<string>();
        for (int i = 0; i < items.Count; i++)
        {
            if (!set.Contains(items[i]))
            {
                result.Add(items[i]);
                set.Add(items[i]);
            }
        }
        return result;
    }
}