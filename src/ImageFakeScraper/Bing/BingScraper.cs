using static System.Net.WebRequestMethods;

namespace ImageFakeScraper.Bing;

public class BingImageFakeScraper : Scraper
{
	private const string uri = "https://www.bing.com/images/search?q={0}&ghsh=0&ghacc=0&first=1&tsc=ImageHoverTitle&cw=2543&ch=1289";
    // &adlt=off

    private const string uri2 = "https://www.bing.com/images/async?q={0}&first=0&count=5000&cw=1177&ch=1289&relp=5000&datsrc=I&layout=RowBased&apc=0&relo=1&relr=6&rely=959&mmasync=1"; // need fix encore ( c'est pas ouf ça ) 

	public async Task<(List<string>, double)> GetImagesAsync(string query, IDatabase redis)
	{
		List<string> tmp = new();
		double dlspeedreturn = 0;
		try
		{
			// get Tag
			string[] args = new string[] { query };
			(HtmlDocument doc, double dlspeed) = await http.Get(uri, args);
			dlspeedreturn = dlspeed;

			HtmlNodeCollection tag = doc.DocumentNode.SelectNodes("//span[@class='suggestion-title']");

            if (tag != null)
			{
				foreach (HtmlNode? item in tag)
				{
					var keyWordClear = HtmlEntity.DeEntitize(item.FirstChild.InnerText);

                    _ = redis.SetAdd("words_list", keyWordClear);
				}
			}

			// Get Img
			(HtmlDocument docs, double dlspeeds) = await http.Get(uri2, args);
			dlspeedreturn += dlspeeds;
			IEnumerable<string> urls = docs.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("src", null)).Where(s => !string.IsNullOrEmpty(s)); // parse url img changer
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
			if (settings.printErrorLog) { Console.WriteLine("Bing" + e); }
		}
		return (tmp, dlspeedreturn);
	}

	public override async Task<(int, double)> GetImages(AsyncCallback ac, params object[] args)
	{
		if (!await redisCheckCount())
		{
			return (0, 0);
		}

		(List<string> urls, double dlspeed) = await GetImagesAsync((string)args[0], (IDatabase)args[4]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		long result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
		TotalPush += result;
		SettingsDll.nbPushTotal += result;

		if (settings.printLog)
		{
			Console.WriteLine("Bing " + result);
		}

		return ((int)result, dlspeed);
	}
}