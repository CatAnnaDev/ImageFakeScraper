using ImageFakeScraperExample.config;

namespace ImageFakeScraperExample.function;

internal class redisImagePush
{
    #region Var
    public static long recordtmp { get; private set; } = 0;
    public static long record { get; private set; } = 0;

    private static readonly List<string> list = new();
    private static readonly List<string> list2 = new();
    #endregion
    #region getAllImage
    public static async Task<long> GetAllImageAndPush(IDatabase conn, Dictionary<string, List<string>> site)
    {
        long data = 0;
        long totalpushactual = 0;
        foreach (KeyValuePair<string, List<string>> image in site)
        {
            if (image.Value != null)
            {
                list.Clear();
                list2.Clear();

                if (Program.ConfigFile.Config.settings.useMongoDB)
                {
                    foreach (string daata in image.Value)
                    {
                        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("hash", CreateMD5(daata));

                        List<BsonDocument> find = Program.Collection.Find(filter).ToList();
                        if (find.Count == 0)
                        {
                            list.Add(daata);
                            BsonDocument document = new() { { "hash", CreateMD5(daata) } };
                            Program.Collection.InsertOne(document);
                        }
                    }
                }
                else
                {
                    image.Value.ForEach(data => list.Add(data));
                }

                foreach (string item in Program.blackList)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Contains(item))
                        {
                            _ = list.Remove(list[i]);
                        }
                    }
                }

                RedisValue[] push = Array.ConvertAll(list.ToArray(), item => (RedisValue)item);
                try
                {
                    Uri opts = new(Program.Credential);

                    RedisValue img_job_ = await conn.SetLengthAsync(Program.key);
                    int img_job_count = int.Parse(img_job_.ToString());

                    RedisValue to_dl = await conn.SetLengthAsync(Program.key);
                    int to_dl_count = int.Parse(to_dl.ToString());


                    if (img_job_count < Program.ConfigFile.Config.settings.stopAfter || to_dl_count < Program.ConfigFile.Config.settings.stopAfter)
                        data = await conn.SetAddAsync(Program.key, push);



                    if (Program.ConfigFile.Config.settings.PrintLog)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        if (image.Key == "DuckDuckGo" || image.Key.Contains("Immerse"))
                        {
                            Console.WriteLine($"{image.Key}:\t{data} / {push.Length}");
                        }
                        else
                        {
                            Console.WriteLine($"{image.Key}:\t\t{data} / {push.Length}");
                        }
                    }

                    totalpushactual += data;
                    if (image.Key == "Pixel")
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
                        if (recordtmp > int.Parse(conn.StringGet(Program.ConfigFile.Config.record_push).ToString().Split(" ").Last()))
                        {
                            record = recordtmp;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"RECORD:\t\t{record}");
                            _ = await conn.StringSetAsync(Program.ConfigFile.Config.record_push, $"{Program.Pseudo} {record}");
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
                    Console.ResetColor();
                }
            }
            else
            {
                if (Program.ConfigFile.Config.settings.PrintLog)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (image.Key == "DuckDuckGo" || image.Key.Contains("Immerse"))
                    {
                        Console.WriteLine($"{image.Key}\tdown");
                    }
                    else
                    {
                        Console.WriteLine($"{image.Key}\t\tdown");
                    }

                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Green;
                    if (image.Key == "Pixel")
                    {
                        Console.WriteLine($"Total:\t\t{totalpushactual}");
                        recordtmp = totalpushactual;
                        totalpushactual = 0;
                    }
                    Console.ResetColor();
                }

                if (recordtmp > record)
                {
                    record = recordtmp;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"RECORD:\t\t{record}");
                    Console.ResetColor();
                }
                try
                {
                    if (recordtmp > int.Parse(conn.StringGet(Program.ConfigFile.Config.record_push).ToString().Split(" ").Last()))
                    {
                        record = recordtmp;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"RECORD:\t\t{record}");
                        _ = await conn.StringSetAsync(Program.ConfigFile.Config.record_push, $"{Program.Pseudo} {record}");
                        Console.ResetColor();
                    }
                }
                catch { }
            }
        }
        return data;
    }
    #endregion
    #region CreateMD5
    public static string CreateMD5(string input)
    {
        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        return Convert.ToHexString(hashBytes);
    }
    #endregion
}
