namespace ImageFakeScraper.Yahoo;

public class YahooScraper : Scraper
{

	private const string uri = "https://images.search.yahoo.com/search/images?ei=UTF-8&p={0}&fr2=p%3As%2Cv%3Ai&.bcrumb=4N2SA8f4BZT&save=0";

	public async Task<(List<string>, double)> GetImagesAsync(string query)
	{
		List<string> tmp = new();
		double dlspeedreturn = 0;
		try
		{
			string[] args = new string[] { query };
			(HtmlDocument doc, double dlspeed) = await http.Get(uri, args);
			dlspeedreturn = dlspeed;
			IEnumerable<string> urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("data-src", null)).Where(s => !string.IsNullOrEmpty(s));

			if (urls == null)
			{
				return (tmp, 0);
			}

			for (int i = 0; i < urls.Count(); i++)
			{
				string cleanUrl = Regex.Replace(urls.ElementAt(i), @"&pid=Api&P=0&w=300&h=300", "");
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
		catch (Exception e) { if (e.GetType().Name != "UriFormatException") { } if (settings.printErrorLog) { Console.WriteLine("Yahoo" + e); } }
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
		SettingsDll.TotalPushYahoo += result;
		SettingsDll.nbPushTotal += result;
		if (settings.printLog)
		{
			Console.WriteLine("Yahoo " + result);
		}

		return ((int)result, dlspeed);
	}
}