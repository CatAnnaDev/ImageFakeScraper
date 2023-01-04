namespace ImageFakeScraperExample;

internal static class Program
{
    #region Var
    private static readonly object ConsoleWriterLock = new();
    public static ConnectionMultiplexer redis;
    public static redisConnection? redisConnector;
    public static Queue<string> qword;
    private static Dictionary<string, List<string>> site;
    private static readonly KestrelMetricServer server = new(port: 4444);
    public static string key;
    public static long totalimageupload = 0;
    public static List<string> blackList = new List<string>();
    private static MongoClient dbClient = new MongoClient("mongodb://localhost:27017/");
    public static IMongoCollection<BsonDocument>? Collection;
    //Thread Pool
    //public static Queue mySyncdQ;
    private static List<Task> tasks = new();
    private static object lockObj = new();
    private static Stopwatch timer = new();
    private static Stopwatch uptime = new();
    private static string text = "";
    private static double waittime = 0;
    private static IDatabase conn;
    private static string[] passArgs;
    private static Random random;
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
    #endregion
    #region Start
    private static async Task Main(string[] args)
    {

        IMongoDatabase dbList = dbClient.GetDatabase("local");
        Collection = dbList.GetCollection<BsonDocument>("cache");

        await dbList.EnsureIndexExists("cache", "hash");

        passArgs = args;
        random = new Random();
        qword = new();

        AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

        string credential = args[0];
        redisConnector = new(credential, 5000);
        redis = redisConnection.redisConnect();
        conn = redis.GetDatabase();

        RedisValue[] bl = await conn.ListRangeAsync("domain_blacklist");
        foreach (RedisValue item in bl)
        {
            blackList.Add(item.ToString());
        }

        waittime = args.Length > 0.1 ? double.Parse(args[1]) : 0;

        if (redis.IsConnected)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Powered by Image Fake Scraper");
            Console.WriteLine("Redis Connected");
            Console.ResetColor();


            RedisKey key = new("words_list");
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

                site = await searchEngineRequest.getAllDataFromsearchEngineAsync(text);
                await redisImagePush.GetAllImageAndPush(conn, site, passArgs);

                if (Settings.GetNewTag)
                {
                    Queue<string> callQword = await searchEngineRequest.getAllNextTag(text, conn);
                    while (callQword.Count != 0)
                    {
                        qword.Enqueue(callQword.Dequeue());
                    }
                }

                site.Clear();


                if (qword.Count <= 2)
                {
                    RedisValue newword = await redisGetNewTag(conn);

                    if (!newword.IsNull && Settings.PrintLogMain)
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
                    if (Program.key != null && Settings.PrintLogMain)
                    {
                        string uptimeFormated = $"{uptime.Elapsed.Days} days {uptime.Elapsed.Hours:00}:{uptime.Elapsed.Minutes:00}:{uptime.Elapsed.Seconds:00}";
                        long redisDBLength = conn.SetLength(Program.key);
                        string redisLength = $"{redisDBLength} / {1_000_000} ({100.0 * redisDBLength / 1_000_000:0.00}%)";
                        double elapsed = TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).TotalSeconds;
                        RedisKey key_done = new("words_done");

                        printData(
                                $"Uptime\t\t{uptimeFormated}\n" +
                                $"Done in\t\t{elapsed}s\n" +
                                $"Sleep\t\t{waittime} sec\n" +
                                $"Memory\t\t{SizeSuffix(usedMemory)}\n" +
                                $"Previous\t{text}\n" +
                                $"NbRequest\t{searchEngineRequest.NbOfRequest}\n" +
                                $"BlackLisst\t{blackList.Count}\n" +
                                $"Tags\t\t{qword.Count}\n" +
                                $"Tag done\t{conn.ListLengthAsync(key_done).Result}\n" +
                                $"Tag remaining\t{conn.ListLengthAsync("words_list").Result}\n" +
                                $"{Program.key}\t{redisLength}\n" +
                                $"Total upload\t{totalimageupload}\n" +
                                $"Record\t\t{redisImagePush.record}\n" +
                                $"Record Glb:\t{conn.StringGet("record_push")}");
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
        BsonDocument index = new BsonDocument
            {
                {indexName, 1}
            };

        CreateIndexModel<BsonDocument> indexModel = new CreateIndexModel<BsonDocument>(index, new CreateIndexOptions { Unique = true });
        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
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
            return await redis.ListLeftPopAsync("words_list");
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
            RedisKey key = new("words_done");
            await redis.ListLeftPushAsync(key, value);
        }
        catch { }
    }
    #endregion
    #region OnProcessExit
    static void OnProcessExit(object sender, EventArgs e)
    {
        Console.WriteLine("redisDisconnet");
        //redisConnection.redisDisconnet();
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
        decimal adjustedSize = (decimal)value / (1L << mag * 10);
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
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
    #endregion
}