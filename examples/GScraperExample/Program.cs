using System;
using System.Collections.Generic;
using System.IO;
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

namespace GScraperExample;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Queue<string> word = new();
        string[] readText = File.ReadAllText("words.txt").Split("\n");
       
        foreach (string s in readText)
        {
            word.Enqueue(s);
        }

        var options = ConfigurationOptions.Parse("imagefake.net:6379");
        options.Password = "yoloimage";
        options.ReconnectRetryPolicy = new ExponentialRetry(10);
        options.CommandMap = CommandMap.Create(new HashSet<string> { "SUBSCRIBE" }, false);
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);

        //write("mot random en cas de besoin", redis);

        Dictionary<string, IEnumerable<IImageResult>> images = new();

        bool ddc = false;
        bool brv = false;

        bool printLog = false;

        IDatabase conn = redis.GetDatabase();

        using var scraper = new GoogleScraper();
        using var duck = new DuckDuckGoScraper();
        using var brave = new BraveScraper();
        HtmlNodeCollection table;
        string actual;

        bool stopBrave = false;

        Queue<string> qword = new();

        var key = new RedisKey("already_done_list");
        var meow = await conn.ListGetByIndexAsync(key, 0);
        string text = meow.ToString();
        qword.Enqueue(text);

        
       // for (int i = 0; i < 10; i++)
       // {
       //     qword.Enqueue(getNewtag(word));
       // }

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
            Console.Out.WriteLineAsync(qword.First());
            Console.Out.WriteLineAsync("=====================================================================");

            while(qword.Count != 0)
            {

                if (GoogleScraper.gg)
                {
                    IEnumerable<IImageResult> google;
                    try
                    {
                        google = await scraper.GetImagesAsync(text);
                        images.Add("Google", google);
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
                        images.Add("DuckDuckGo", duckduck);

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
                        images.Add("Brave", bravelist);
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
                        //ddc = true;
                    }
                    if (!brv)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Brave stopped");
                        Console.ResetColor();
                        //brv = true;
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
                                            if (await Read(redis, table[j].InnerText) == -1)
                                            {
                                                qword.Enqueue(table[j].InnerText);
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.Out.WriteLineAsync($"Tag Added {table[j].InnerText}");
                                                Console.ResetColor();
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
                    if (image.Value != null)
                    {

                        var list = new List<string>();

                        foreach (var daata in image.Value)
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

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{image.Key} Images found: {list.Count}");
                            Console.ResetColor();
                        }
                        catch { Console.ForegroundColor = ConsoleColor.Red; Console.Out.WriteLineAsync("Fail upload redis !"); Console.ResetColor(); }
                    }
                    else
                    {
                        Console.Out.WriteLineAsync("Image is null fix it yourself !");
                    }
                }
                images.Clear();

                if (qword.Count <= 2)
                {
                    var newword = await getNewtag(redis);
                    qword.Enqueue(newword.ToString());
                    text = qword.Dequeue();
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"Missing tag pick a new random: {newword}");
                    Console.ResetColor();
                }
                else
                {
                    text = qword.Dequeue();
                }
                
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
                    Console.Out.WriteLineAsync($"Previous\t{text}\nNext\t\t{qword.ToArray()[qword.Count-1]}\nTags\t\t{qword.Count}\nRedis Length\t{conn.SetLength("image_jobs")} / {uint.MaxValue} ({(100.0 * (float)conn.SetLength("image_jobs") / (float)uint.MaxValue).ToString("0.000")}%)\nWords Length\t{await redis.GetDatabase().ListLengthAsync(key)}");
                }
                catch
                {
                    Console.Out.WriteLineAsync($"Previous\t{text}\nTags\t\t{qword.Count}\nRedis Length\t{conn.SetLength("image_jobs")} / {uint.MaxValue} ({(100.0 * (float)conn.SetLength("image_jobs") / (float)uint.MaxValue).ToString("0.000")}%)\nWords Length\t{await redis.GetDatabase().ListLengthAsync(key)}");
                }
                Console.Out.WriteLineAsync("================================================================================================================================");
                Console.Out.WriteLineAsync($"Sleep {waittime}sec;");
                Thread.Sleep(TimeSpan.FromSeconds(waittime));
            }
        }
    }


    private static async Task<RedisValue> getNewtag(ConnectionMultiplexer redis) => await redis.GetDatabase().ListLeftPopAsync("words_list");

    private static async void write(string text, ConnectionMultiplexer redis)
    {
        var value = new RedisValue(text);
        var key = new RedisKey("already_done_list");
        await redis.GetDatabase().ListLeftPushAsync(key, value);
    }

    private static async Task<long> Read(ConnectionMultiplexer redis, string text) => await redis.GetDatabase().ListPositionAsync("already_done_list", text);
}