namespace GScraperExample.function;

internal class redisImagePush
{
    #region Var
    private static readonly bool printLog = false;
    public static long recordtmp { get; private set; } = 0;
    public static long record { get; private set; } = 0;
    #endregion

    #region getAllImage
    public static async Task<long> GetAllImageAndPush(IDatabase conn, Dictionary<string, List<string>> site, string[] args)
    {
        long data = 0;
        long totalpushactual = 0;
        foreach (KeyValuePair<string, List<string>> image in site)
        {
            if (image.Value != null)
            {
                List<string> list = new();

                foreach (var daata in image.Value)
                {
                    list.Add(daata);
                }

                foreach (var item in Program.blackList)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Contains(item))
                        {
                            list.Remove(list[i]);
                        }

                    }
                }

                RedisValue[] push = Array.ConvertAll(list.ToArray(), item => (RedisValue)item);
                try
                {

                    Uri opts = new(args[0]);
                    List<RedisKey> redisList = redisConnection.GetServers.Keys(0, "*image_jobs*").ToList();

                    var nextIndex = await conn.StringGetAsync("jobs_last_index");
                    var parseKey = int.Parse(nextIndex.ToString());
                    Program.key = $"image_jobs_{parseKey}";


                    if (conn.SetLength(Program.key) >= 1_000_000)
                    {
                        Program.key = $"image_jobs_{parseKey + 1}";
                        await conn.StringSetAsync("jobs_last_index", parseKey + 1);
                    }


                    data = await conn.SetAddAsync(Program.key, push);
                    Console.ForegroundColor = ConsoleColor.Green;
                    if (image.Key == "DuckDuckGo")
                        Console.WriteLine($"{image.Key}:\t{data} / {push.Length}");
                    else
                        Console.WriteLine($"{image.Key}:\t\t{data} / {push.Length}");

                    totalpushactual += data;
                    if (image.Key == "Getty")
                    {
                        Console.WriteLine($"Total:\t\t{totalpushactual}");
                        recordtmp = totalpushactual;
                        totalpushactual = 0;
                    }
                    Console.ResetColor();
                    if (recordtmp > record)
                    {
                        record = recordtmp;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"RECORD:\t\t{record}");
                        Console.ResetColor();
                    }
                    try
                    {
                        if (recordtmp > int.Parse(conn.StringGet("record_push").ToString().Split(" ").Last()))
                        {
                            record = recordtmp;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"RECORD:\t\t{record}");
                            await conn.StringSetAsync("record_push", $"{args[2]} {record}");
                            Console.ResetColor();
                        }
                    }
                    catch { }
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
                if (image.Key == "DuckDuckGo")
                    Console.WriteLine($"{image.Key}\tdown");
                else
                    Console.WriteLine($"{image.Key}\t\tdown");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Green;

                if (image.Key == "Getty")
                {
                    Console.WriteLine($"Total:\t\t{totalpushactual}");
                    recordtmp = totalpushactual;
                    totalpushactual = 0;
                }
                Console.ResetColor();
                if (recordtmp > record)
                {
                    record = recordtmp;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"RECORD:\t\t{record}");
                    Console.ResetColor();
                }
                try
                {
                    if (recordtmp > int.Parse(conn.StringGet("record_push").ToString().Split(" ").Last()))
                    {
                        record = recordtmp;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"RECORD:\t\t{record}");
                        await conn.StringSetAsync("record_push", $"{args[2]} {record}");
                        Console.ResetColor();
                    }
                }
                catch { }
            }
        }
        return data;
    }
    #endregion


}
