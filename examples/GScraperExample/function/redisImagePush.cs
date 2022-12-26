using GScraper;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GScraperExample.function
{
    internal class redisImagePush
    {
        static bool printLog = false;
        static long data = 0;

        public static async Task<long> GetAllImageAndPush(IDatabase conn, Dictionary<string, IEnumerable<IImageResult>> site)
        {
            foreach (var image in site)
            {
                if (image.Value != null)
                {

                    var list = new List<string>();

                    foreach (var daata in image.Value)
                    {
                        if (printLog)
                        {
                            Console.WriteLine();
                            Console.WriteLine(JsonSerializer.Serialize(daata, daata.GetType(), new JsonSerializerOptions { WriteIndented = true }));
                            Console.WriteLine(daata.ToString());
                        }
                        list.Add(daata.Url);

                        if (printLog)
                            Console.WriteLine();
                    }

                    var parse = list.ToArray();
                    var push = Array.ConvertAll(parse, item => (RedisValue)item);
                    try
                    {
                        data = await conn.SetAddAsync("image_jobs", push);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{image.Key} Images found: {data}");
                        Console.ResetColor();
                        
                    }
                    catch { Console.ForegroundColor = ConsoleColor.Red; await Console.Out.WriteLineAsync("Fail upload redis !"); Console.ResetColor();}
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
