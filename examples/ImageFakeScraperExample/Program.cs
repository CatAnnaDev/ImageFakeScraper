using System.Threading;

namespace ImageFakeScraperExample;

internal static class Program
{
    #region Var
    public static ConnectionMultiplexer? redis;
    public static redisConnection? redisConnector;
    public static string? key;
    public static string? key_to_dl;
    public static buildJsonFile? ConfigFile;

    // Config File
    public static string Credential = "";
	public static double waittime = 0;
    public static int nbThread;
    public static int QueueLimit;
    public static int requestMaxPerSec = 8;

	#endregion
	#region Start
	[Obsolete]
    private static async Task Main(string[] args)
    {
        ConfigFile = new();
        await InitializeGlobalDataAsync();

        Credential = ConfigFile.Config.Credential;
		requestMaxPerSec = ConfigFile.Config.requestMaxPerSec;
        nbThread = ConfigFile.Config.nbThread;
		waittime = (1.0 / (double)requestMaxPerSec) * nbThread;
		QueueLimit = ConfigFile.Config.QueueLimit;
        key = ConfigFile.Config.images_jobs;
        key_to_dl = ConfigFile.Config.to_download;

        if (Credential == "Redis Login")
        {
            Console.WriteLine($"Update config file \n{Directory.GetCurrentDirectory()}\\Config.json");
            return;
        }

        redisConnector = new(Credential, 5000);
        redis = redisConnection.redisConnect();

        if (redis.IsConnected)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Powered by Image Fake Scraper");
            Console.WriteLine("Redis Connected");
            Console.ResetColor();

            MultiThread multi = new(ConfigFile.Config.settings.PrintLog, ConfigFile.Config.settings.PrintLogTag, nbThread, QueueLimit);

            Console.WriteLine("InitMultiThread");
            multi.InitMultiThread();
            //Console.WriteLine("Search");
            //multi.Search(await redisGetNewTag(redisConnection.GetDatabase));
            Console.WriteLine("SpawnThreads");
            multi.SpawnThreads();
/*            Console.WriteLine("Wait");
            Console.Read();*/

        }
        Console.WriteLine("Done");
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
    private static async Task InitializeGlobalDataAsync()
    {
        await ConfigFile.InitializeAsync();
    }
}