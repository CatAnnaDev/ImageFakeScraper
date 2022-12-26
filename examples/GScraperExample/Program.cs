using GScraper.Brave;
using GScraper.DuckDuckGo;
using GScraper.Google;
using GScraperExample.function;
using HtmlAgilityPack;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GScraperExample;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Queue<string> word = new();

        //string[] readText = File.ReadAllText("words.txt").Split("\n");

        //foreach (string s in readText)
        //{
        //    word.Enqueue(s);
        //}

        var opts = new Uri(args[0]);
        var credentials = opts.UserInfo.Split(':');
        var options = ConfigurationOptions.Parse($"{opts.Host}:{opts.Port},password={credentials[1]},user={credentials[0]}");
        options.ReconnectRetryPolicy = new ExponentialRetry(10);
        options.CommandMap = CommandMap.Create(new HashSet<string> { "SUBSCRIBE" }, false);
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);

        //write("mot random en cas de besoin", redis);


        bool printLog = false;

        IDatabase conn = redis.GetDatabase();

        using var scraper = new GoogleScraper();
        using var duck = new DuckDuckGoScraper();
        using var brave = new BraveScraper();
        HtmlNodeCollection table;

        Queue<string> qword = new();

        var key = new RedisKey("already_done_list");
        var meow = await conn.ListGetByIndexAsync(key, 0);
        string text = meow.ToString();
        qword.Enqueue(text);

        long totalimageupload = 0;

        double waittime;
        if (args.Length > 0.1)
            waittime = double.Parse(args[1]);
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

            Console.ForegroundColor = ConsoleColor.Green;
            await Console.Out.WriteLineAsync("Redis Connected");
            Console.ResetColor();

            await Console.Out.WriteLineAsync("=====================================================================");
            await Console.Out.WriteLineAsync(qword.First());
            await Console.Out.WriteLineAsync("=====================================================================");

            Stopwatch timer = new();

            Stopwatch uptime = new();
            uptime.Start();

            while (qword.Count != 0)
            {
                timer.Start();

                var site = await searchEngineRequest.getAllDataFromsearchEngineAsync(text);

                var callQword = await searchEngineRequest.getAllNextTag(text, redis);

                while (callQword.Count != 0)
                    qword.Enqueue(callQword.Dequeue()); // euh


                foreach (var image in site)
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
                            var truc = await conn.SetAddAsync("image_jobs", push);
                            totalimageupload += truc;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{image.Key} Images found: {truc}");
                            Console.ResetColor();
                        }
                        catch { Console.ForegroundColor = ConsoleColor.Red; await Console.Out.WriteLineAsync("Fail upload redis !"); Console.ResetColor(); }
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync("Image is null fix it yourself !");
                    }
                }
                site.Clear();

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
                    await Console.Out.WriteLineAsync("redis disconnected, press enter to stop");
                    Console.ResetColor();
                    Console.ReadLine();
                    break;
                }

                if (conn.SetLength("image_jobs") == uint.MaxValue - 10000)
                {
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        await Console.Out.WriteLineAsync($"Redis queue alomst full {conn.ListLength("image_jobs")}");
                        Console.ResetColor();
                        Console.ReadLine();
                    }
                    catch { }
                }

                try
                {
                    write(text, redis);
                }
                catch { }

                timer.Stop();
                var uptime2 = $"{uptime.Elapsed.Days} days {uptime.Elapsed.Hours.ToString("00")}:{uptime.Elapsed.Minutes.ToString("00")}:{uptime.Elapsed.Seconds.ToString("00")}";
                await Console.Out.WriteLineAsync("================================================================================================================================");

                try
                {
                    await Console.Out.WriteLineAsync(
                         $"Uptime\t\t{uptime2}\n" +
                         $"Done in\t\t{timer.ElapsedMilliseconds} ms\n" +
                         $"Previous\t{text}\nNext\t\t{qword.ToArray()[qword.Count - 1]}\nT" +
                         $"ags\t\t{qword.Count}\n" +
                         $"Redis Length\t{conn.SetLength("image_jobs")} / {uint.MaxValue} ({(100.0 * (float)conn.SetLength("image_jobs") / (float)uint.MaxValue).ToString("0.000")}%)\n" +
                         $"Words Length\t{await redis.GetDatabase().ListLengthAsync(key)}\n" +
                         $"Total upload\t{totalimageupload}\n" +
                         $"Sleep\t\t{waittime} sec");
                }
                catch
                {
                    await Console.Out.WriteLineAsync(
                        $"Uptime\t\t{uptime2}\n" +
                        $"Done in\t\t{timer.ElapsedMilliseconds} ms\n" +
                        $"Previous\t{text}\n" +
                        $"Tags\t\t{qword.Count}\n" +
                        $"Redis Length\t{conn.SetLength("image_jobs")} / {uint.MaxValue} ({(100.0 * (float)conn.SetLength("image_jobs") / (float)uint.MaxValue).ToString("0.000")}%)\n" +
                        $"Words Length\t{await redis.GetDatabase().ListLengthAsync(key)}\n" +
                        $"Total upload\t{totalimageupload}\n" +
                        $"Sleep\t\t{waittime} sec");
                }
                await Console.Out.WriteLineAsync("================================================================================================================================");
                Thread.Sleep(TimeSpan.FromSeconds(waittime));
                timer.Reset();
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
}