using GScraper.Brave;
using GScraper.DuckDuckGo;
using GScraper.Google;
using GScraperExample.function;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GScraperExample;

internal static class Program
{
    private static ConnectionMultiplexer? redis;
    public static Queue<string>? qword;

    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

    private delegate bool EventHandler(CtrlType sig);

    private static EventHandler? _handler;
    public static long totalimageupload = 0;

    private enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    private static bool HandlerAsync(CtrlType sig)
    {
        Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");
        Console.WriteLine("Upload all tag in redis in progress");

        while (qword.Count != 0)
        {
            _ = redis.GetDatabase().ListLeftPush("words_list", qword.Dequeue());
        }

        Console.WriteLine("All done press enter to exit");
        _ = Console.ReadLine();
        Environment.Exit(-1);
        return true;
    }


    private static async Task Main(string[] args)
    {
        qword = new();

        _handler += new EventHandler(HandlerAsync);
        _ = SetConsoleCtrlHandler(_handler, true);

        Queue<string> word = new();

        //string[] readText = File.ReadAllText("words.txt").Split("\n");

        //foreach (string s in readText)
        //{
        //    word.Enqueue(s);
        //}

        Uri opts = new(args[0]);
        string[] credentials = opts.UserInfo.Split(':');
        ConfigurationOptions options = ConfigurationOptions.Parse($"{opts.Host}:{opts.Port},password={credentials[1]},user={credentials[0]}");
        options.ReconnectRetryPolicy = new ExponentialRetry(10);
        options.CommandMap = CommandMap.Create(new HashSet<string> { "SUBSCRIBE" }, false);
        redis = ConnectionMultiplexer.Connect(options);

        //write("mot random en cas de besoin", redis);

        IDatabase conn = redis.GetDatabase();

        using GoogleScraper scraper = new();
        using DuckDuckGoScraper duck = new();
        using BraveScraper brave = new();



        RedisKey key = new("already_done_list");
        RedisValue meow = await conn.ListGetByIndexAsync(key, 0);
        string text = meow.ToString();
        qword.Enqueue(text);

        

        double waittime = args.Length > 0.1 ? double.Parse(args[1]) : 0;
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

                Dictionary<string, IEnumerable<GScraper.IImageResult>> site = await searchEngineRequest.getAllDataFromsearchEngineAsync(text);

                Queue<string> callQword = await searchEngineRequest.getAllNextTag(text, redis);

                while (callQword.Count != 0)
                {
                    qword.Enqueue(callQword.Dequeue()); // euh
                }

                long tr = await redisImagePush.GetAllImageAndPush(conn, site);
                //totalimageupload += tr;

                site.Clear();

                if (qword.Count <= 2)
                {
                    RedisValue newword = await getNewtag(redis);
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
                    _ = Console.ReadLine();
                    break;
                }

                if (conn.SetLength("image_jobs") == uint.MaxValue - 10000)
                {
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        await Console.Out.WriteLineAsync($"Redis queue alomst full {conn.ListLength("image_jobs")}");
                        Console.ResetColor();
                        _ = Console.ReadLine();
                    }
                    catch { }
                }

                try
                {
                    write(text, redis);
                }
                catch { }

                timer.Stop();
                string uptime2 = $"{uptime.Elapsed.Days} days {uptime.Elapsed.Hours:00}:{uptime.Elapsed.Minutes:00}:{uptime.Elapsed.Seconds:00}";
                await Console.Out.WriteLineAsync("================================================================================================================================");

                try
                {
                    await Console.Out.WriteLineAsync(
                         $"Uptime\t\t{uptime2}\n" +
                         $"Done in\t\t{timer.ElapsedMilliseconds} ms\n" +
                         $"Previous\t{text}\nNext\t\t{qword.ToArray()[qword.Count - 1]}\nT" +
                         $"ags\t\t{qword.Count}\n" +
                         $"Redis Length\t{conn.SetLength("image_jobs")} / {uint.MaxValue} ({100.0 * conn.SetLength("image_jobs") / uint.MaxValue:0.000}%)\n" +
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
                        $"Redis Length\t{conn.SetLength("image_jobs")} / {uint.MaxValue} ({100.0 * conn.SetLength("image_jobs") / uint.MaxValue:0.000}%)\n" +
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

    private static async Task<RedisValue> getNewtag(ConnectionMultiplexer redis)
    {
        return await redis.GetDatabase().ListLeftPopAsync("words_list");
    }

    private static async void write(string text, ConnectionMultiplexer redis)
    {
        RedisValue value = new(text);
        RedisKey key = new("already_done_list");
        _ = await redis.GetDatabase().ListLeftPushAsync(key, value);
    }
}