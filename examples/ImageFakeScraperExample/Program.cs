namespace ImageFakeScraperExample;
#pragma warning disable CS8602, CS8604, CS8618, CS1634
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
	public static Settings ConfigSettings;

	#endregion
	#region Start
	[Obsolete]
	private static async Task Main(string[] args)
	{
		ConfigFile = new();
		await InitializeGlobalDataAsync();


        Credential = ConfigFile.Configs["Credential"].ToString();
        requestMaxPerSec = (int)ConfigFile.Configs["requestMaxPerSec"];
        nbThread = (int)ConfigFile.Configs["nbThread"];
        waittime = 1.0 / requestMaxPerSec * nbThread;
        QueueLimit = (int)ConfigFile.Configs["QueueLimit"];
        key = ConfigFile.Configs["images_jobs"].ToString();
        key_to_dl = ConfigFile.Configs["to_download"].ToString();

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
			Console.WriteLine(FiggleFonts.Standard.Render("Crawler"));

			MultiThread multi = new((bool)ConfigFile.Configs["settings"]["PrintLog"], (bool)ConfigFile.Configs["settings"]["PrintLogTag"], nbThread, QueueLimit);

			multi.InitMultiThread();
			multi.SpawnThreads();

		}
		Console.WriteLine("");
	}
	#endregion

	public static async Task InitializeGlobalDataAsync()
	{
		await ConfigFile.InitializeAsync();
    }
}