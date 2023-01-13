using ImageFakeScraperExample.config;

namespace ImageFakeScraperExample;

internal static class Program
{
    #region Var
    private static readonly object ConsoleWriterLock = new();
    public static ConnectionMultiplexer? redis;
    public static redisConnection? redisConnector;
    public static Queue<string>? qword;
    private static readonly KestrelMetricServer server = new(port: 4444);
    public static string? key;
    public static string? key_to_dl;
    public static long totalimageupload = 0;
    public static List<string> blackList = new();
    private static readonly MongoClient dbClient = new("mongodb://localhost:27017/");
    public static IMongoCollection<BsonDocument>? Collection;
    public static buildJsonFile? ConfigFile;

    // Config File
    public static string Credential = "";
    private static double waittime = 0;
    public static string Pseudo = "";

    //Thread Pool
    //public static Queue mySyncdQ;
    private static readonly List<Task> tasks = new();
    private static readonly object lockObj = new();
    private static readonly Stopwatch timer = new();
    private static readonly Stopwatch uptime = new();
    private static string text = "";

    private static IDatabase? conn;
    private static Random? random;
    private static readonly SemaphoreSlim _lock = new(initialCount: 1, maxCount: 1);
    #endregion
    #region Start
    [Obsolete]
    private static async Task Main(string[] args)
    {
        ConfigFile = new();
        await InitializeGlobalDataAsync();

        Credential = ConfigFile.Config.Credential;
        waittime = ConfigFile.Config.Sleep;
        Pseudo = ConfigFile.Config.Pseudo;
        key = ConfigFile.Config.images_jobs;
        key_to_dl = ConfigFile.Config.to_download;

        // Bloom filter 

        //int capacity = 2000000;
        //var filter = new Bloom.Bloom<string>(capacity);
        //filter.Add("content");
        //
        //if (filter.Contains("content"))
        //{
        //    return;
        //}

        if (Credential == "Redis Login")
        {
            Console.WriteLine($"Update config file \n{Directory.GetCurrentDirectory()}\\Config.json");
            return;
        }

        if (ConfigFile.Config.settings.useMongoDB)
        {
            IMongoDatabase dbList = dbClient.GetDatabase("local");
            Collection = dbList.GetCollection<BsonDocument>("cache");

            await dbList.EnsureIndexExists("cache", "hash");
        }

        random = new Random();
        qword = new();

        redisConnector = new(Credential, 5000);
        redis = redisConnection.redisConnect();
        conn = redis.GetDatabase();

        RedisValue[] bl = await conn.ListRangeAsync(ConfigFile.Config.domain_blacklist);
        foreach (RedisValue item in bl)
        {
            blackList.Add(item.ToString());
        }

        if (redis.IsConnected)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Powered by Image Fake Scraper");
            Console.WriteLine("Redis Connected");
            Console.ResetColor();


            RedisKey key = new(ConfigFile.Config.words_list);
            long rng = await conn.ListLengthAsync(key);
            random = new Random();
            RedisValue getredisValue = await conn.ListGetByIndexAsync(key, random.Next(0, (int)rng - 1));
            text = getredisValue.ToString();
            qword.Enqueue(text);
            printData(text);

            while (qword.Count != 0)
            {
                Process currentProcess = Process.GetCurrentProcess();
                long usedMemory = currentProcess.PrivateMemorySize64;

                timer.Start();
                uptime.Start();

                await searchEngineRequest.getAllDataFromsearchEngineAsync(text);
                //_ = await redisImagePush.GetAllImageAndPush(conn, site);

                if (ConfigFile.Config.settings.GetNewTagGoogle)
                {
                    Queue<string> callQword = await searchEngineRequest.getAllNextTag(text, conn);
                    while (callQword.Count != 0)
                    {
                        qword.Enqueue(callQword.Dequeue());
                    }
                }

                if (qword.Count <= 2)
                {
                    RedisValue newword = await redisGetNewTag(conn);

                    if (!newword.IsNull && ConfigFile.Config.settings.PrintLogMain)
                    {

                        qword.Enqueue(newword.ToString());
                        text = qword.Dequeue();
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"Missing tag pick a new random: {newword}");
                        Console.ResetColor();
                    }
                }
                else
                {

                    text = qword.Dequeue();
                }


                redisWriteNewTag(text, conn);

                timer.Stop();

                try
                {
                    if (Program.key != null && ConfigFile.Config.settings.PrintLogMain)
                    {
                        string uptimeFormated = $"{uptime.Elapsed.Days} days {uptime.Elapsed.Hours:00}:{uptime.Elapsed.Minutes:00}:{uptime.Elapsed.Seconds:00}";
                        long redisDBLength = conn.SetLength(Program.key);
                        string redisLength = $"{redisDBLength} / {ConfigFile.Config.settings.stopAfter} ({100.0 * redisDBLength / ConfigFile.Config.settings.stopAfter:0.00}%)";

                        long redisDBLengthto_dl = conn.SetLength(key_to_dl);
                        string redisLengthto_dl = $"{redisDBLengthto_dl} / {ConfigFile.Config.settings.stopAfter} ({100.0 * redisDBLengthto_dl / ConfigFile.Config.settings.stopAfter:0.00}%)";


                        double elapsed = TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).TotalSeconds;


                        printData(
                                $"Uptime\t\t{uptimeFormated}\n" +
                                $"Done in\t\t{elapsed}s\n" +
                                $"Sleep\t\t{waittime} sec\n" +
                                $"Memory\t\t{SizeSuffix(usedMemory)}\n" +
                                $"Previous\t{text}\n" +
                                $"BlackList\t{blackList.Count}\n" +
                                $"Tags\t\t{qword.Count}\n" +
                                $"Tag done\t{conn.ListLengthAsync(ConfigFile.Config.words_done).Result}\n" +
                                $"Tag remaining\t{conn.ListLengthAsync(ConfigFile.Config.words_list).Result}\n" +
                                $"{Program.key}\t{redisLength}\n" +
                                $"{key_to_dl}\t{redisLengthto_dl}\n");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }



                Thread.Sleep(TimeSpan.FromSeconds(waittime));

                timer.Reset();
                searchEngineRequest.NbOfRequest = 0;
            }
        }
        Console.WriteLine("Done");
    }
    #endregion
    #region EnsureIndexExists
    private static async Task EnsureIndexExists(this IMongoDatabase database, string collectionName, string indexName)
    {
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(collectionName);
        BsonDocument index = new()
        {
                {indexName, 1}
            };

        CreateIndexModel<BsonDocument> indexModel = new(index, new CreateIndexOptions { Unique = true });
        _ = await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
    }
    #endregion
    #region printData
    private static void printData(string text)
    {
        lock (ConsoleWriterLock)
        {
            string line = string.Concat(Enumerable.Repeat("=", Console.WindowWidth));
            Console.WriteLine(line);
            Console.WriteLine(text);
            Console.WriteLine(line);
        }
    }
    #endregion
    #region redisGetNewTag
    private static async Task<RedisValue> redisGetNewTag(IDatabase redis)
    {
        try
        {
            return await redis.ListLeftPopAsync(ConfigFile.Config.words_list);
        }
        catch { return RedisValue.Null; }
    }
    #endregion
    #region redisWriteNewTag
    private static async void redisWriteNewTag(string text, IDatabase redis)
    {
        try
        {
            RedisValue value = new(text);
            RedisKey key = new(ConfigFile.Config.words_done);
            _ = await redis.ListLeftPushAsync(key, value);
        }
        catch { }
    }
    #endregion
    #region SizeSuffix
    private static string SizeSuffix(long value, int decimalPlaces = 1)
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
    #endregion
    #region ShuffleArray
    public static void Shuffle<T>(this Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            (array[k], array[n]) = (array[n], array[k]);
        }
    }
    #endregion

    private static async Task InitializeGlobalDataAsync()
    {
        await ConfigFile.InitializeAsync();
    }
}