using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GScraper;
using GScraper.Brave;
using GScraper.DuckDuckGo;
using GScraper.Google;
using HtmlAgilityPack;
using StackExchange.Redis;

namespace GScraperExample;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        using var scraper = new GoogleScraper();
        using var duck = new DuckDuckGoScraper();
        using var brave = new BraveScraper();
        HtmlNodeCollection table;
        string actual;

        List<string> qword = new();

        string? text = args[0];
        qword.Add(text);

        int waittime;
        if (args.Length > 1)
            waittime = int.Parse(args[1]);
        else
            waittime = 5;

        var options = ConfigurationOptions.Parse("imagefake.net:6379");
        options.Password = "yoloimage";
        options.CommandMap = CommandMap.Create(new HashSet<string> { "SUBSCRIBE" }, false);
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);

        IDatabase conn = redis.GetDatabase();

        if (redis.IsConnected)
        {
            await Console.Out.WriteLineAsync("Redis Connected");

            await Console.Out.WriteLineAsync("=====================================================================");
            await Console.Out.WriteLineAsync(qword[0]);
            await Console.Out.WriteLineAsync("=====================================================================");

            for (int i = 0; i < qword.Count; i++)
            {
                qword.Distinct().ToList();

                IEnumerable<IImageResult> google;
                try
                {
                    google = await scraper.GetImagesAsync(text);
                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.WriteLine(e);
                    continue;
                }

                IEnumerable<IImageResult> duckduck;
                try
                {
                    duckduck = await duck.GetImagesAsync(text);
                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.WriteLine(e);
                    continue;
                }

                IEnumerable<IImageResult> bravelist;
                try
                {
                    bravelist = await brave.GetImagesAsync(text);
                }
                catch (Exception e) when (e is HttpRequestException or GScraperException)
                {
                    Console.WriteLine(e);
                    continue;
                }

                var images = new List<IEnumerable<IImageResult>>
               {
                  bravelist,
                  duckduck,
                  google
               };

                var url = $"https://www.google.com/search?q={text}&tbm=isch&hl=en";
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = client.GetAsync(url).Result)
                    {
                        using (HttpContent content = response.Content)
                        {
                            string result = content.ReadAsStringAsync().Result;
                            HtmlDocument document = new();
                            document.LoadHtml(result);

                            table = document.DocumentNode.SelectNodes("//a[@class='TwVfHd']");

                            try
                            {
                                if (table != null)
                                {
                                    for (var j = 0; j < table.Count; j++)
                                    {
                                        qword.Add(table[j].InnerText);
                                        //qword.Distinct().ToList();
                                    }
                                    var listduplicate = RemoveDuplicatesSet(qword);
                                    qword.Clear();
                                    qword.AddRange(listduplicate);
                                }
                            }
                            catch (Exception e)
                            {
                                await Console.Out.WriteLineAsync("No Tag!");
                                await Console.Out.WriteLineAsync(e.Message);
                            }
                        }
                    }
                }

                foreach (var image in images)
                {
                    image.Distinct().ToList();

                    foreach (var daata in image)
                    {
                        Console.WriteLine();
                        Console.WriteLine(JsonSerializer.Serialize(daata, daata.GetType(), new JsonSerializerOptions { WriteIndented = true })); ;
                        redis.GetDatabase().ListRightPush("image_hash_jobs", daata.Url);
                        Console.WriteLine();

                    }
                }
                images.Clear();
                text = qword[i];

                if (!redis.IsConnected)
                {
                    await Console.Out.WriteLineAsync("redis disconnected, press enter to stop");
                    Console.ReadLine();
                    break;
                }
                if (table == null)
                    await Console.Out.WriteLineAsync("No more Tag found!");

                await Console.Out.WriteLineAsync("=====================================================================");
                await Console.Out.WriteLineAsync($"Previous done: {text}, Next: {qword[i+1]}");
                await Console.Out.WriteLineAsync("=====================================================================");
                await Console.Out.WriteLineAsync($"Sleep {waittime}sec;");
                Thread.Sleep(TimeSpan.FromSeconds(waittime));
            }
        }
    }

    public static List<string> RemoveDuplicatesSet(List<string> items)
    {
        if (items.Count == 1)
            return items;

        var result = new List<string>();
        var set = new HashSet<string>();
        for (int i = 0; i < items.Count; i++)
        {
            if (!set.Contains(items[i]))
            {
                result.Add(items[i]);
                set.Add(items[i]);
            }
        }
        return result;
    }
}