using Microsoft.VisualBasic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageFakeScraperExample
{
	public class MultiThread
	{

		private SemaphoreSlim mySemaphoreSlim = new SemaphoreSlim(1, 1);

		private AutoResetEvent auto = new(false);

		private Dictionary<string, Scraper> dicoEngine = new();

		private static Object _lock = new();

		ConcurrentQueue<Tuple<string, string>> queue = new();

		private List<Thread> threadList = new();

		private static readonly List<Task> tasks = new();

		int ThreadCount = 0;

		int QueueLimit = 30;

		bool printLog;
		bool printLogTag;

		private static readonly Stopwatch uptime = new();

		public MultiThread(bool printLog, bool printLogTag, int nbThread = 8, int QueueLimit = 30)
		{
			ThreadCount = nbThread;
			this.QueueLimit = QueueLimit;
			this.printLog = printLog;
			this.printLogTag = printLogTag;
		}

		public void InitMultiThread()
		{
			uptime.Start();

			if (Program.ConfigFile.Config.settings.BingRun)
				dicoEngine.Add("Bing", new BinImageFakeScraper());
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
				if (queue.Count() < QueueLimit)
				{
					for (int i = 0; i < QueueLimit - queue.Count(); i++)
					{
						var keyword = await redisConnection.GetDatabase.SetPopAsync(Program.ConfigFile.Config.words_list);
						Search(keyword);
						if (printLogTag)
						{
							lock (_lock)
							{

								Console.ForegroundColor = ConsoleColor.Magenta;
								Console.WriteLine($"Keyword pick: {keyword}");
								Console.ResetColor();
							}
						}
					}
				}
				Thread.Sleep(5000);
			}
		}

		private void LogPrintData()
		{
			while (true)
			{
				try
				{
					string uptimeFormated = $"{uptime.Elapsed.Days} days {uptime.Elapsed.Hours:00}:{uptime.Elapsed.Minutes:00}:{uptime.Elapsed.Seconds:00}";
					printData(
						$"Uptime\t\t\t{uptimeFormated}\n" +
						$"Total Tag\t\t{queue.Count}\n");
				}
				catch { }

				Thread.Sleep(TimeSpan.FromMinutes(1));
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

			Thread GlobalLog = new Thread(LogPrintData);
			GlobalLog.Start();
		}

		private async void Worker()
		{
			while (true)
			{
				try
				{
					Tuple<string, string>? work;

					if (queue.TryDequeue(out work))
					{
						var name = work.Item1;
						var keyword = work.Item2;
						Console.WriteLine(keyword + queue.Count);
						//if (dicoEngine.TryGetValue(name, out Scraper? engine))
						Random rand = new Random();
						dicoEngine = dicoEngine.OrderBy(x => rand.Next()).ToDictionary(item => item.Key, item => item.Value);
						for (int i = 0; i < dicoEngine.Count; i++)
						{

							mySemaphoreSlim.Wait();
							try
							{
								object[] args = new object[] { keyword, 1, 1_500, false, redisConnection.GetDatabase };
								var urls = dicoEngine.ElementAt(i).Value.GetImages(args);
								RedisPush(urls.Result, dicoEngine.ElementAt(i).Key, printLog);
							}
							finally
							{
								mySemaphoreSlim.Release();
							}
						}
					}
					//Console.WriteLine("j'arriv pas queue");
				}
				catch (Exception e) { Console.WriteLine(e); }

				Thread.Sleep(TimeSpan.FromSeconds(Program.ConfigFile.Config.Sleep));
			}
		}

		private static async Task RedisPush(List<string> urls, string moteur, bool printLog)
		{
			try
			{
				int dataPrint = 0;
				RedisValue img_job_ = await redisConnection.GetDatabase.SetLengthAsync(Program.ConfigFile.Config.images_jobs);
				int img_job_count = int.Parse(img_job_.ToString());

				RedisValue to_dl = await redisConnection.GetDatabase.SetLengthAsync(Program.ConfigFile.Config.to_download);
				int to_dl_count = int.Parse(to_dl.ToString());

				if (img_job_count < Program.ConfigFile.Config.settings.stopAfter && to_dl_count < Program.ConfigFile.Config.settings.stopAfter)
				{
					RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);

					await redisConnection.GetDatabase.SetAddAsync(Program.key, push);

					if (printLog)
					{

						Console.ForegroundColor = ConsoleColor.Green;
						if (moteur.Contains("Immerse"))
						{
							Console.WriteLine($"{moteur}:\t{urls.Count}");

						}
						else
						{
							Console.WriteLine($"{moteur}:\t\t{urls.Count}");
						}
						Console.ResetColor();
					}
					dataPrint += urls.Count;
				}
				else
				{
					while (true)
					{
						RedisValue img_job_check = await redisConnection.GetDatabase.SetLengthAsync(Program.ConfigFile.Config.images_jobs);
						int img_job_count_check = int.Parse(img_job_check.ToString());

						RedisValue to_dl_check = await redisConnection.GetDatabase.SetLengthAsync(Program.ConfigFile.Config.to_download);
						int to_dl_count_check = int.Parse(to_dl_check.ToString());

						if (img_job_count < Program.ConfigFile.Config.settings.stopAfter && to_dl_count < Program.ConfigFile.Config.settings.stopAfter)
							break;

						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($" Wait Redis full");
						Console.ResetColor();
						for (int a = 60; a >= 0; a--)
						{
							Thread.Sleep(1000);
							Console.Write($"\rWait after retry {TimeSpan.FromMinutes(a)}, img_job_count: {img_job_count_check}, to_dl_count: {to_dl_count_check}");
						}

						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write($"\n\rWait Redis full retry! {DateTime.Now.ToString("G")}");
						Console.ResetColor();

					}
				}
			}
			catch (Exception e)
			{

				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"/!\\ Fail upload redis {moteur} ! /!\\" + e.Message);
				Console.ResetColor();
			}
		}

		private static void printData(string text)
		{
			lock (_lock)
			{
				string line = string.Concat(Enumerable.Repeat("=", Console.WindowWidth));
				Console.WriteLine(line);
				Console.WriteLine(text);
				Console.WriteLine(line);
			}
		}
	}
}

