
namespace GScraperExample.function;

internal class redisConnection
{
    #region Var
    static string credential = "";
    static int exponentialRetry = 0;
    static ConnectionMultiplexer Connection { get; set; }
    public static IServer GetServers { get; set; }
    public static IDatabase GetDatabase { get; set; }
    #endregion

    #region Constuctor
    public redisConnection(string loggin, int ExponentialRetry)
    {
        credential = loggin;
        exponentialRetry = ExponentialRetry;
    }
    #endregion

    #region Login
    public static ConnectionMultiplexer redisConnect()
    {
        Uri opts = new(credential);
        string[] credentials = opts.UserInfo.Split(':');
        ConfigurationOptions options = ConfigurationOptions.Parse($"{opts.Host}:{opts.Port},password={credentials[1]},user={credentials[0]}");
        options.AbortOnConnectFail = false;
        options.AsyncTimeout = int.MaxValue;
        options.ConnectTimeout = int.MaxValue;
        options.ResponseTimeout = int.MaxValue;
        options.SyncTimeout = int.MaxValue;
        options.ReconnectRetryPolicy = new ExponentialRetry(exponentialRetry);
        options.CommandMap = CommandMap.Create(new HashSet<string> { "SUBSCRIBE" }, false);
        TextWriter log = Console.Out;
        Connection = ConnectionMultiplexer.Connect(options, log);
        GetServers = Connection.GetServer($"{opts.Host}:{opts.Port}");
        GetDatabase = Connection.GetDatabase();
        return Connection;
    }
    #endregion

    #region disconnect
    public static async void redisDisconnet()
    {
        await Connection.CloseAsync();
    }
    #endregion
}

