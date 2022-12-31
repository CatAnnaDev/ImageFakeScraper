namespace GScraperExample.function;

internal class redisImagePush
{
    #region Var
    private static readonly bool printLog = false;
    public static long recordtmp { get; private set; } = 0;
    public static long record { get; private set; } = 0;
    #endregion

    #region getAllImage
    public static async Task<long> GetAllImageAndPush(IDatabase conn, Dictionary<string, IEnumerable<IImageResult>> site, string[] args)
    {
        long data = 0;
        long totalpushactual = 0;
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
                        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(daata, daata.GetType(), new JsonSerializerOptions { WriteIndented = true }));
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
                    List<RedisKey> redisList = redisConnection.GetServers.Keys(0, "*image_jobs*").ToList();
                    List<string> sorted = redisList.Select(key => key.ToString()).ToList();
                    sorted.Sort();

                    for (int y = 0; y < redisList.Count; y++)
                    {
                        string[] lastList = redisList[y].ToString().Split("_");
                        tmp = int.Parse(lastList.Last());
                        if (DBnum < tmp)
                        {
                            DBnum = tmp;
                            Program.key = redisList[y].ToString();
                        }
                    }

                    if (conn.SetLength(Program.key) >= 1_000_000)
                    {

                        string[] lastLists = Program.key.ToString().Split("_");
                        if (conn.SetLength(Program.key) >= 1_000_000)
                        {
                            int parse = int.Parse(lastLists.Last());
                            Program.key = $"{lastLists[0]}_{lastLists[1]}_{int.Parse(lastLists[2]) + 1}";
                        }
                    }

                    data = await conn.SetAddAsync(Program.key, push);
                    Console.ForegroundColor = ConsoleColor.Green;
                    if (image.Key == "Openverse")
                        Console.WriteLine($"{image.Key}:\t{data} / {push.Length}");
                    else if (image.Key == "DuckDuckGo")
                        Console.WriteLine($"{image.Key}:\t{data} / {push.Length}");
                    else
                        Console.WriteLine($"{image.Key}:\t\t{data} / {push.Length}");

                    totalpushactual += data;
                    if (image.Key == "Every")
                    {
                        Console.WriteLine($"Total:\t\t{totalpushactual}");
                        recordtmp = totalpushactual;
                        totalpushactual = 0;
                    }
                    Console.ResetColor();
                    if (recordtmp > long.Parse(conn.StringGet("record_push")))
                    {
                        record = recordtmp;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"RECORD:\t\t{record}");
                        await conn.StringSetAsync("record_push", record.ToString());
                        Console.ResetColor();
                    }
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
                    //redisConnection.redisConnect();
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

                Console.ForegroundColor = ConsoleColor.Green;

                if (image.Key == "Every")
                {
                    Console.WriteLine($"Total:\t\t{totalpushactual}");
                    recordtmp = totalpushactual;
                    totalpushactual = 0;
                }
                Console.ResetColor();
                if (recordtmp > long.Parse(conn.StringGet("record_push")))
                {
                    record = recordtmp;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"RECORD:\t\t{record}");
                    await conn.StringSetAsync("record_push", record.ToString());
                    Console.ResetColor();
                }
            }
        }
        return data;
    }
    #endregion
}
