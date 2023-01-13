using System;

namespace ImageFakeScraperExample
{
	public class MultiThread
	{
        private AutoResetEvent auto = new(false);

        private Dictionary<string, Scraper> dicoEngine = new();

        private static Object _lock = new();

        ConcurrentQueue<Tuple<string, string>> queue = new();

        private List<Thread> threadList = new();

        int ThreadCount = 0;

        public static long TotalPush = 0;

        int QueueLimit = 30;

        bool printLog;
        bool printLogTag;

        public MultiThread(bool printLog, bool printLogTag, int nbThread = 8, int QueueLimit = 30)
		{            
            ThreadCount = nbThread;
            this.QueueLimit = QueueLimit;
            this.printLog = printLog;
            this.printLogTag = printLogTag;
        }

        public void InitMultiThread()
        {
            if (Program.ConfigFile.Config.settings.GoogleRun)
                dicoEngine.Add("Google", new GoogleScraper());
            if (Program.ConfigFile.Config.settings.DuckduckGORun)
                dicoEngine.Add("Duck", new DuckDuckGoScraper());
            if (Program.ConfigFile.Config.settings.BraveRun)
                dicoEngine.Add("Brave", new BraveScraper());
            if (Program.ConfigFile.Config.settings.AlamyRun)
                dicoEngine.Add("Alamy", new AlamyScraper());
            if (Program.ConfigFile.Config.settings.OpenVerseRun)
                dicoEngine.Add("Open", new OpenVerseScraper());
            if (Program.ConfigFile.Config.settings.BingRun)
                dicoEngine.Add("Bing", new BinImageFakeScraper());
            if (Program.ConfigFile.Config.settings.YahooRun)
                dicoEngine.Add("Yahoo", new YahooScraper());
            if (Program.ConfigFile.Config.settings.GettyImageRun)
                dicoEngine.Add("Getty", new GettyScraper());
            if (Program.ConfigFile.Config.settings.EveryPixelRun)
                dicoEngine.Add("Pixel", new PixelScraper());
            if (Program.ConfigFile.Config.settings.ImmerseRun)
                dicoEngine.Add("Immerse", new ImmerseScraper());



        }

        private async void PollKeywords()
        {
            while (true)
            {
                if(queue.Count() < QueueLimit)
                {
                    for (int i = 0; i < QueueLimit - queue.Count(); i++)
                    {
                        var keyword = await redisConnection.GetDatabase.ListLeftPopAsync(Program.ConfigFile.Config.words_list);
                        Search(keyword);
                        if (printLogTag)
                        {
                            lock (_lock)
                            {
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.WriteLine($"Keyword pick: {keyword} / Keywords Count: {queue.Count()}");
                                Console.ResetColor();
                            }
                        }
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public void Search(string keyword)
        {
            foreach (var kvp in dicoEngine)
            {
                queue.Enqueue(new Tuple<string, string>(kvp.Key, keyword));
            }
        }

        public void SpawnThreads()
        {
            for (int i = 0; i < ThreadCount; i++)
            {
                Thread thread1 = new Thread(Worker);
                threadList.Add(thread1);
                thread1.Start();
            }

            Thread poll = new Thread(PollKeywords);
            poll.Start();
        }

        private async void Worker()
        {
            while (true)
            {

                Tuple<string, string>? work;

                if (queue.TryDequeue(out work))
                {
                    var name = work.Item1;
                    var keyword = work.Item2;

                    if(dicoEngine.TryGetValue(name, out Scraper? engine))
                    {
                        object[] args = new object[] { keyword, 25, 1500, false };
                        var urls = await engine.GetImages(args);
                        await RedisPush(urls, name, printLog);
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(Program.ConfigFile.Config.Sleep));
            }

        }

        private static async Task RedisPush(List<string> moteur, string Name, bool printLog)
        {
            try
            {
                RedisValue img_job_ = await redisConnection.GetDatabase.SetLengthAsync(Program.ConfigFile.Config.images_jobs);
                int img_job_count = int.Parse(img_job_.ToString());

                RedisValue to_dl = await redisConnection.GetDatabase.SetLengthAsync(Program.ConfigFile.Config.to_download);
                int to_dl_count = int.Parse(to_dl.ToString());

                if (img_job_count < Program.ConfigFile.Config.settings.stopAfter && to_dl_count < Program.ConfigFile.Config.settings.stopAfter)
                {
                    RedisValue[] push = Array.ConvertAll(moteur.ToArray(), item => (RedisValue)item);

                    long data = await redisConnection.GetDatabase.SetAddAsync(Program.key, push);
                    if (printLog)
                    {
                        lock (_lock)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"engine {Name} / Data {data}");
                            Console.ResetColor();
                        }
                    }
                }
                else
                {
                    while (true)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Wait Redis full");
                        Console.ResetColor();
                        for (int a = 120; a >= 0; a--)
                        {
                            Thread.Sleep(1000);
                            Console.Write($"\rWait after retry {TimeSpan.FromMinutes(a)}");
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"\rWait Redis full retry! {DateTime.Now.ToString("G")}");
                        Console.ResetColor();

                        RedisValue img_job_check = await redisConnection.GetDatabase.SetLengthAsync(Program.ConfigFile.Config.images_jobs);
                        int img_job_count_check = int.Parse(img_job_check.ToString());

                        RedisValue to_dl_check = await redisConnection.GetDatabase.SetLengthAsync(Program.ConfigFile.Config.to_download);
                        int to_dl_count_check = int.Parse(to_dl_check.ToString());

                        if (img_job_count < Program.ConfigFile.Config.settings.stopAfter && to_dl_count < Program.ConfigFile.Config.settings.stopAfter)
                            break;
                    }
                }
            }
            catch
            {
                lock (_lock)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"/!\\ Fail upload redis {Name} ! /!\\");
                    Console.ResetColor();
                }
            }
        }
    }
}

