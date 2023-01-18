#pragma warning disable CS8602, CS8604, CS8618, CS1634, CS8600
namespace ImageFakeScraper.Qwant
{
	public class QwantScraper : Scraper
	{

		private const string uri = "https://api.qwant.com/v3/search/images?q={0}&count=250&offset=0&locale=fr_fr&s=0";

		public async Task<(List<string>, double)> GetImagesAsync(string query)
		{
			List<string> tmp = new();
			double dlspeedreturn = 0;
			try
			{
				string[] args = new string[] { query.Replace(" ", "%20") };
				(string jsonGet, double dlspeed) = await http.GetJson(uri, args);
				dlspeedreturn = dlspeed;
				Root jsonparsed = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(jsonGet);

				if (jsonparsed == null || jsonparsed.data == null || jsonparsed.data.result == null || jsonparsed.data.result.items.Count == 0)
				{
					return (tmp, 0);
				}

				for (int j = 0; j < jsonparsed.data.result.items.Count; j++)
				{
					Uri truc = new(jsonparsed.data.result.items[j].media);
					if (truc == null)
					{
						continue;
					}

					tmp.Add(jsonparsed.data.result.items[j].media);
				}

			}
			catch (Exception e)
			{
				if (e.GetType().Name != "UriFormatException") { }
				if (settings.printErrorLog) { Console.WriteLine("Qwant" + e); }
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
			SettingsDll.nbPushTotal += result;
			if (settings.printLog)
			{
				Console.WriteLine("Qwant " + result);
			}

			return ((int)result, dlspeed);
		}
	}
}

