﻿using GScraper;
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
        
        public static async Task<long> GetAllImageAndPush(IDatabase conn, Dictionary<string, IEnumerable<IImageResult>> site)
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

                        if (printLog)
                        {
                            Console.WriteLine();
                        }
                    }

                    //string[] parse = list.ToArray();
                    RedisValue[] push = Array.ConvertAll(list.ToArray(), item => (RedisValue)item);
                    try
                    {
                        data = await conn.SetAddAsync("image_jobs", push);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{image.Key} Images found: {data}");
                        Console.ResetColor();
                        Program.totalimageupload += data;
                        data = 0;

                    }
                    catch { Console.ForegroundColor = ConsoleColor.Red; await Console.Out.WriteLineAsync("Fail upload redis !"); Console.ResetColor(); }
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
