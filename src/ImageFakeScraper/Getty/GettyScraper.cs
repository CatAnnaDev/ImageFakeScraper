namespace ImageFakeScraper.Getty;

public class GettyScraper : Scraper
{
	private const string uri = "https://www.gettyimages.fr/photos/{0}?assettype=image&excludenudity=false&license=rf&family=creative&phrase={1}&sort=mostpopular&page={2}";

	public async Task<(List<string>, double)> GetImagesAsync(string query, int GettyMaxPage)
	{
		List<string> tmp = new();
		double dlspeedreturn = 0;
		try
		{
			for (int i = 1; i < GettyMaxPage + 1; i++)
			{
				object[] args = new object[] { query, query, i.ToString() };
				(HtmlDocument doc, double dlspeed) = await http.Get(uri, args);
				dlspeedreturn = dlspeed;
				IEnumerable<string> urls = doc.DocumentNode.Descendants("source").Select(e => e.GetAttributeValue("srcSet", null)).Where(s => !string.IsNullOrEmpty(s));

				if (urls.Count() == 0)
				{
					break;
				}

				for (int j = 0; j < urls.Count(); j++)
				{
					Uri truc = new(urls.ElementAt(j));
					if (truc == null)
					{
						continue;
					}

					tmp.Add(urls.ElementAt(j).Replace("&amp;", "&"));
				}
			}
		}
		catch (Exception e)
		{
			if (e.GetType().Name != "UriFormatException") { }
			if (settings.printErrorLog) { Console.WriteLine("Getty" + e); }
		}
		return (tmp, dlspeedreturn);
	}

	public override async Task<(int, double)> GetImages(AsyncCallback ac, params object[] args)
	{
		if (!await redisCheckCount())
		{
			return (0, 0);
		}

		(List<string> urls, double dlspeed) = await GetImagesAsync((string)args[0], (int)args[1]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		long result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
		TotalPush += result;
		SettingsDll.nbPushTotal += result;
		if (settings.printLog)
		{
			Console.WriteLine("Getty " + result);
		}

		return ((int)result, dlspeed);
	}
}