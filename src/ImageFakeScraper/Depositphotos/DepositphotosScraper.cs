
namespace ImageFakeScraper.Depositphotos
{
	public class DepositphotosScraper : Scraper
	{
		// private const string uri = "https://depositphotos.com/stock-photos/{0}.html?filter=all"; // 403 
		// private const string uri = "https://www.istockphoto.com/search/2/image?excludenudity=false&phrase={0}&sort=best"; // login page
		// private const string uri = "https://pixabay.com/images/search/{0}/?manual_search=1"; // js

		private const string uri = "https://pixabay.com/images/search/{0}/?manual_search=1";

		public async Task<(List<string>, double)> GetImagesAsync(string query)
		{
			query = query.Replace(" ", "-");
			List<string> tmp = new();
			double dlspeedreturn = 0;
			try
			{
				string[] args = new string[] { query };
				(HtmlDocument doc, double dlspeed) = await http.Get(uri, args);
				dlspeedreturn = dlspeed;
				IEnumerable<string> urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("src", null)).Where(s => !string.IsNullOrEmpty(s));

				for (int i = 0; i < urls.Count(); i++)
				{
					string cleanUrl = Regex.Replace(urls.ElementAt(i), @"[?&][^?&]+=[^?&]+", "");
					if (cleanUrl.EndsWith("th") || urls.ElementAt(i).Contains("th?id="))
					{
						continue;
					}

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
				if (settings.printErrorLog) { Console.WriteLine("Deposit" + e); }
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
				Console.WriteLine("Deposit " + result);
			}

			return ((int)result, dlspeed);
		}
	}
}
