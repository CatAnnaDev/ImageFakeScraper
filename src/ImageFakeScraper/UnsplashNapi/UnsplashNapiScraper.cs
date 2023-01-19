#pragma warning disable CS8602, CS8604, CS8618, CS1634, CS8600

namespace ImageFakeScraper.UnsplashNapi
{
	public class UnsplashNapiScraper : Scraper
	{
		private const string uri = "https://unsplash.com/napi/search/photos?query={0}&page=1&per_page=100000";

		public async Task<(List<string>, double)> GetImagesAsync(string query)
		{
			List<string> tmp = new();
			double dlspeedreturn = 0;
			try
			{
				string[] args = new string[] { query };
				(string jsonGet, double dlspeed) = await http.GetJson(uri, args);
				dlspeedreturn = dlspeed;
				Root jsonparsed = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(jsonGet);

				if (jsonparsed == null || jsonparsed.results == null || jsonparsed.results.Count == 0)
				{
					return (tmp, 0);
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
				if (settings.printErrorLog) { Console.WriteLine("UnsplashNapi" + e); }
			}
			return (tmp, dlspeedreturn);
		}

		public override async Task<(int, double)> GetImages(AsyncCallback ac, params object[] args)
		{

			if (!await redisCheckCount())
			{
				return (0, 0);
			}

			(List<string> urls, double dlspeed) = await GetImagesAsync((string)args[0]);
			RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);

			long result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
			TotalPush += result;
			SettingsDll.nbPushTotal += result;
			if (settings.printLog)
			{
				Console.WriteLine("UnsplashNapi " + result);
			}

			return ((int)result, dlspeed);
		}

	}
}

