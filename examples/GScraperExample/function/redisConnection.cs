using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace GScraperExample.function
{
    internal class redisConnection
    {
        static string credential = "";
        static int exponentialRetry = 0;
        public redisConnection(string loggin, int ExponentialRetry)
        {
            credential = loggin;
            exponentialRetry = ExponentialRetry;
        }

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
            return ConnectionMultiplexer.Connect(options);
        }
    }
}

