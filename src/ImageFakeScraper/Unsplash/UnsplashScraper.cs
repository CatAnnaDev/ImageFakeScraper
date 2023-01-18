#pragma warning disable CS8602, CS8604, CS8618, CS1634, CS8600
namespace ImageFakeScraper.Unsplash
{
	public class UnsplashScraper : Scraper
	{
		private const string uri = "https://unsplash.com/napi/search/photos?query={0}&page=1&per_page=10000";

		public async Task<List<string>> GetImagesAsync(string query)
		{
			List<string> tmp = new();
			try
			{
				string[] args = new string[] { query };
				string jsonGet = await http.GetJson(uri, args);
				Root jsonparsed = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(jsonGet);

				if (jsonparsed == null || jsonparsed.results == null || jsonparsed.results.Count == 0)
				{
					return tmp;
				}

				for (int i = 0; i < jsonparsed.results.Count; i++)
				{
					string cleanUrl = Regex.Replace(jsonparsed.results[i].urls.raw, @"[?&][^?&]+=[^?&]+", "");
					Uri truc = new(cleanUrl);
					if (truc == null)
					{
						continue;
					}

					tmp.Add(cleanUrl);
				}

			}
			catch (Exception e)
			{
				if (e.GetType().Name != "UriFormatException") { }
				if (settings.printErrorLog) { Console.WriteLine("Unsplash" + e); }
			}
			return tmp;
		}

		public override async Task<int> GetImages(AsyncCallback ac, params object[] args)
		{

			if (!await redisCheckCount())
			{
				return 0;
			}

			List<string> urls = await GetImagesAsync((string)args[0]);
			RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);

			long result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
			SettingsDll.nbPushTotal += result;
			if (settings.printLog)
			{
				Console.WriteLine("Qwant " + result);
			}

			return (int)result;
		}

	}
}

