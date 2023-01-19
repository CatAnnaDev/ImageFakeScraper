#pragma warning disable CS8602, CS8604, CS8618, CS1634, CS8600

namespace ImageFakeScraper.UnsplashNgetty
{
	public class UnsplashScraperngetty : Scraper
	{
		private const string uri = "https://unsplash.com/ngetty/v3/search/images/creative?fields=display_set%2Creferral_destinations%2Ctitle&page_size=100&phrase={0}&sort_order=best_match&graphical_styles=images&exclude_nudity=false&exclude_editorial_use_only=false";

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

				if (jsonparsed == null || jsonparsed.images == null || jsonparsed.images.Count == 0)
				{
					return (tmp, 0);
				}

				for (int i = 0; i < jsonparsed.images.Count; i++)
				{
					for (int j = 0; j < jsonparsed.images[i].display_sizes.Count; j++)
					{
						Uri truc = new(jsonparsed.images[i].display_sizes[j].uri);
						if (truc == null)
						{
							continue;
						}

						tmp.Add(jsonparsed.images[i].display_sizes[j].uri);
					}

				}
			}
			catch (Exception e)
			{
				if (e.GetType().Name != "UriFormatException") { }
				if (settings.printErrorLog) { Console.WriteLine("UnsplashNgetty" + e); }
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
				Console.WriteLine("UnsplashNgetty " + result);
			}

			return ((int)result, dlspeed);
		}

	}
}

