using GScraper;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GScraperExample.function
{
    internal class redisImagePush
    {
        private static readonly bool printLog = false;

        public static async Task<long> GetAllImageAndPush(ConnectionMultiplexer conn, Dictionary<string, IEnumerable<IImageResult>> site)
        {
            long data = 0;
            foreach (KeyValuePair<string, IEnumerable<IImageResult>> image in site)
            {
                if (image.Value != null)
                {

                    List<string> list = new();

                    foreach (IImageResult daata in image.Value)
                    {
                        if (printLog)
                        {
                            Console.WriteLine();
                            Console.WriteLine(JsonSerializer.Serialize(daata, daata.GetType(), new JsonSerializerOptions { WriteIndented = true }));
                            Console.WriteLine(daata.ToString());
                        }
                        list.Add(daata.Url);
                    }

                    RedisValue[] push = Array.ConvertAll(list.ToArray(), item => (RedisValue)item);
                    try
                    {
                        if (conn.GetDatabase().SetLength(Program.key) >= 1_000_000)
                        {
                            var pattern = new RedisValue("DB0");
                            var redisList = conn.GetServer("imagefake.net:6379").Keys(0, "*image_jobs*").ToArray();
                            if(conn.GetDatabase().SetLength(redisList.First()) >= 1_000_000)
                            {
                                var lastList = redisList.First().ToString().Split("_");
                                var parse = int.Parse(lastList.Last());
                                Program.key = $"{lastList[0]}_{lastList[1]}_{parse+1}";
                            }
                            else
                            {
                                Program.key = redisList.First();
                            }

                        }

                            data = await conn.GetDatabase().SetAddAsync(Program.key, push);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{image.Key} Images found: {data}");
                            Console.ResetColor();
                            Program.totalimageupload += data;
                            data = 0;

                    }
                    catch { 
                        Console.ForegroundColor = ConsoleColor.Red; 
                        await Console.Out.WriteLineAsync("/!\\ Fail upload redis ! /!\\");
                        await Console.Out.WriteLineAsync("/!\\ Fail upload redis ! /!\\");
                        await Console.Out.WriteLineAsync("/!\\ Fail upload redis ! /!\\");
                        Console.ResetColor();

                        await Console.Out.WriteLineAsync("/!\\ Reconnecting to redis server ! /!\\");
                        redisConnection.redisConnect();
                    }
                }
                else
                {
                    await Console.Out.WriteLineAsync("Image is null fix it yourself !");
                }
            }
            return data;
        }
    }
}
