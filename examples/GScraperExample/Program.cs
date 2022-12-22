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

            for (int i = 0; i < qword.Count; i++)
            {
                qword.Distinct().ToList();
                await Console.Out.WriteLineAsync("=====================================================================");
                await Console.Out.WriteLineAsync(qword[i]);
                await Console.Out.WriteLineAsync("=====================================================================");

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
                                    foreach (var data in table)
                                    {
                                        foreach(var truc in qword)
                                        {
                                            if(data.InnerText != truc)
                                                qword.Add(data.InnerText);
                                        }
                                    }
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

                await Console.Out.WriteLineAsync($"Sleep {waittime}sec;");
                Thread.Sleep(TimeSpan.FromSeconds(waittime));
            }
        }
    }
}