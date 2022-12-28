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
using Prometheus;

namespace GScraperExample;

internal static class Program
{
    private static readonly object ConsoleWriterLock = new object();
    public static ConnectionMultiplexer? redis;
    public static redisConnection redisConnector;
    public static Queue<string>? qword;
    private static Dictionary<string, IEnumerable<GScraper.IImageResult>> site;
    private static KestrelMetricServer server = new KestrelMetricServer(port: 4444);
    public static string key = "image_jobs_0";


    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

    private delegate bool EventHandler(CtrlType sig);

    private static EventHandler? handler;
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

        using GoogleScraper scraper = new();
        using DuckDuckGoScraper duck = new();
        using BraveScraper brave = new();

        qword = new();
        
        handler += new EventHandler(HandlerAsync);
        _ = SetConsoleCtrlHandler(handler, true);

        //Queue<string> word = new();
        //string[] readText = File.ReadAllText("words.txt").Split("\n");

        //foreach (string s in readText)
        //{
        //    word.Enqueue(s);
        //}

        string credential = args[0];
        redisConnection redisConnector = new redisConnection(credential, 5000);
        var redis = redisConnection.redisConnect();
        IDatabase conn = redis.GetDatabase();

        //write("mot random en cas de besoin", redis);

        RedisKey key = new("words_done");
        RedisValue getredisValue = await conn.ListGetByIndexAsync(key, 0);
        string text = getredisValue.ToString();
        qword.Enqueue(text);

        double waittime = args.Length > 0.1 ? double.Parse(args[1]) : 0;

        if (redis.IsConnected)
        {

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Redis Connected");
            Console.ResetColor();

            printData(qword.First());

            Stopwatch timer = new();

            Stopwatch uptime = new();
            uptime.Start();

            while (qword.Count != 0)
            {
                timer.Start();


                Process currentProcess = Process.GetCurrentProcess();
                long usedMemory = currentProcess.PrivateMemorySize64;

                site = await searchEngineRequest.getAllDataFromsearchEngineAsync(text);

                Queue<string> callQword = await searchEngineRequest.getAllNextTag(text, redis);

                while (callQword.Count != 0)
                    qword.Enqueue(callQword.Dequeue()); // euh

                _ = await redisImagePush.GetAllImageAndPush(redis, site, args);

                site.Clear();

                if (qword.Count <= 2)
                {
                    RedisValue newword = await redisGetNewTag(redis);
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
                    printData("redis disconnected");
                    Console.ResetColor();
                }
                else
                {
                    while (!redis.IsConnected)
                    {
                        await Console.Out.WriteLineAsync("/!\\ Reconnecting to redis server ! 10sec /!\\");
                        redisConnection.redisConnect();
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }

                }

                if (conn.SetLength("image_jobs_0") == uint.MaxValue - 10000)
                {
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Redis queue alomst full {conn.ListLength("image_jobs_0")}");
                        Console.ResetColor();
                        _ = Console.ReadLine();
                    }
                    catch { }
                }

                redisWriteNewTag(text, redis);

                timer.Stop();

                string uptimeFormated = $"{uptime.Elapsed.Days} days {uptime.Elapsed.Hours:00}:{uptime.Elapsed.Minutes:00}:{uptime.Elapsed.Seconds:00}";
                long redisDBLength = conn.SetLength(Program.key);
                string redisLength = $"{redisDBLength} / {1_000_000} ({100.0 * redisDBLength / 1_000_000:0.00}%)";

                printData(
                        $"Uptime\t\t{uptimeFormated}\n" +
                        $"Done in\t\t{timer.ElapsedMilliseconds} ms\n" +
                        $"Sleep\t\t{waittime} sec\n" +
                        $"Memory\t\t{SizeSuffix(usedMemory)}\n"+
                        $"Previous\t{text}\n" +
                        $"Tags\t\t{qword.Count}\n" +
                        $"{Program.key}\t{redisLength}\n" +
                        $"Tag done\t{await redis.GetDatabase().ListLengthAsync(key)}\n" +
                        $"Tag remaining\t{await redis.GetDatabase().ListLengthAsync("words_list")}\n" +
                        $"Total upload\t{totalimageupload}\n");


                Thread.Sleep(TimeSpan.FromSeconds(waittime));

                timer.Reset();
            }
        }
        else
        {
            while (!redis.IsConnected)
            {
                await Console.Out.WriteLineAsync("/!\\ Reconnecting to redis server ! 10sec /!\\");
                redisConnection.redisConnect();
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
            
        }
    }

    private static void printData(string text)
    {
        lock (ConsoleWriterLock)
        {
            Console.WriteLine("========================================================");
            Console.WriteLine(text);
            Console.WriteLine("========================================================");
        }
    }

    private static async Task<RedisValue> redisGetNewTag(ConnectionMultiplexer redis) => await redis.GetDatabase().ListLeftPopAsync("words_list");

    private static async void redisWriteNewTag(string text, ConnectionMultiplexer redis)
    {
        RedisValue value = new(text);
        RedisKey key = new("words_done");
        await redis.GetDatabase().ListLeftPushAsync(key, value);
    }

    static string SizeSuffix(long value, int decimalPlaces = 1)
    {
        string[] SizeSuffixes = { "bytes", "KB", "MB", "GB" };
        if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
        if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
        if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }
        int mag = (int)Math.Log(value, 1024);
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));
        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        { mag += 1; adjustedSize /= 1024; }
        return string.Format("{0:n" + decimalPlaces + "} {1}",
            adjustedSize,
            SizeSuffixes[mag]);
    }
}