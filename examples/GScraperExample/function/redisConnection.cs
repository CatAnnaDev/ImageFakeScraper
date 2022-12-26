using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GScraperExample.function
{
    internal class redisConnection
    {
        static string credential = "";
        static ConnectionMultiplexer redis;
        public redisConnection(string loggin)
        {
            credential = loggin;
        }

        public ConnectionMultiplexer redisConnect()
        {
            Uri opts = new(credential);
            string[] credentials = opts.UserInfo.Split(':');
            ConfigurationOptions options = ConfigurationOptions.Parse($"{opts.Host}:{opts.Port},password={credentials[1]},user={credentials[0]}");
            options.ReconnectRetryPolicy = new ExponentialRetry(10);
            options.CommandMap = CommandMap.Create(new HashSet<string> { "SUBSCRIBE" }, false);
            return  ConnectionMultiplexer.Connect(options);
        }
    }
}
