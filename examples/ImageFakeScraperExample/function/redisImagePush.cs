using ImageFakeScraperExample.config;
using static System.Net.Mime.MediaTypeNames;

namespace ImageFakeScraperExample.function;

internal class redisImagePush
{
    #region Var
    private static readonly List<string> list = new();
    private static RedisValue[] push;
    private static long data = 0;
    private static string WebSite = "";
    #endregion
    #region getAllImage
    public static async Task GetAllImageAndPush(IDatabase conn, List<string> site, string Site)
    {
        WebSite = Site;
        list.Clear();

            if (site != null)
            {
               
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

                push = Array.ConvertAll(site.ToArray(), item => (RedisValue)item);
                try
                {
                    Uri opts = new(Program.Credential);

                    RedisValue img_job_ = await conn.SetLengthAsync(Program.key);
                    int img_job_count = int.Parse(img_job_.ToString());

                    RedisValue to_dl = await conn.SetLengthAsync(Program.key);
                    int to_dl_count = int.Parse(to_dl.ToString());


                    if (img_job_count < Program.ConfigFile.Config.settings.stopAfter || to_dl_count < Program.ConfigFile.Config.settings.stopAfter)
                        data = await conn.SetAddAsync(Program.key, push);

                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    await Console.Out.WriteLineAsync($"/!\\ Fail upload redis {Site} ! /!\\");
                    Console.ResetColor();
                }
            }
            else
            {
                if (Program.ConfigFile.Config.settings.PrintLog)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (Site == "DuckDuckGo" || Site.Contains("Immerse"))
                    {
                        Console.WriteLine($"{Site}\tdown");
                    }
                    else
                    {
                        Console.WriteLine($"{Site}\t\tdown");
                    }

                    Console.ResetColor();
                }
            }

        if (Program.ConfigFile.Config.settings.PrintLog)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (WebSite == "DuckDuckGo" || WebSite.Contains("Immerse"))
            {
                Console.WriteLine($"{WebSite}:\t{data} / {push.Length}");
            }
            else
            {
                Console.WriteLine($"{WebSite}:\t\t{data} / {push.Length}");
            }
        }
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
