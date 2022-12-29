using GScraper;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GScraperExample.function
{
    internal class redisImagePush
    {
        private static readonly bool printLog = false;

        public static async Task<long> GetAllImageAndPush(ConnectionMultiplexer conn, Dictionary<string, IEnumerable<IImageResult>> site, string[] args)
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
                        int DBnum = 0;
                        int tmp = 0;
                        Uri opts = new(args[0]);
                        var redisList = conn.GetServer($"{opts.Host}:{opts.Port}").Keys(0, "*image_jobs*").ToList();
                        var sorted = redisList.Select(key => key.ToString()).ToList();
                        sorted.Sort();

                        for (int y = 0; y < redisList.Count; y++)
                        {
                            var lastList = redisList[y].ToString().Split("_");
                            tmp = int.Parse(lastList.Last());
                            if (DBnum < tmp)
                            {
                                DBnum = tmp;
                                Program.key = redisList[y].ToString();
                            }
                        }

                        if (conn.GetDatabase().SetLength(Program.key) >= 1_000_000)
                        {

                            var lastLists = Program.key.ToString().Split("_");
                            if (conn.GetDatabase().SetLength(Program.key) >= 1_000_000)
                            {

                                var parse = int.Parse(lastLists.Last());
                                Program.key = $"{lastLists[0]}_{lastLists[1]}_{int.Parse(lastLists[2]) + 1}";
                            }
                            else
                            {
                                //Program.key = sorted.Last();
                            }

                        }

                        data = await conn.GetDatabase().SetAddAsync(Program.key, push);
                        Console.ForegroundColor = ConsoleColor.Green;
                        if (image.Key == "Openverse")
                            Console.WriteLine($"{image.Key}:\t{data} / {push.Length}");
                        else if(image.Key == "DuckDuckGo")
                            Console.WriteLine($"{image.Key}:\t{data} / {push.Length}");
                        else
                            Console.WriteLine($"{image.Key}:\t\t{data} / {push.Length}");
                        Console.ResetColor();
                        Program.totalimageupload += data;
                        data = 0;

                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        await Console.Out.WriteLineAsync($"/!\\ Fail upload redis {image.Key} ! /!\\");
                        await Console.Out.WriteLineAsync($"/!\\ Fail upload redis {image.Key} ! /!\\");
                        await Console.Out.WriteLineAsync($"/!\\ Fail upload redis {image.Key} ! /!\\");
                        Console.ResetColor();

                        await Console.Out.WriteLineAsync("/!\\ Reconnecting to redis server ! /!\\");
                        redisConnection.redisConnect();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (image.Key == "Openverse")
                        Console.WriteLine($"{image.Key}\tdown");
                    else if (image.Key == "DuckDuckGo")
                        Console.WriteLine($"{image.Key}\tdown");
                    else
                        Console.WriteLine($"{image.Key}\t\tdown");
                    Console.ResetColor();
                }
            }
            return data;
        }
    }
}
