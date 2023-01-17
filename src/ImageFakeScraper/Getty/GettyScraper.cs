namespace ImageFakeScraper.Getty;

public class GettyScraper : Scraper
{

	private const string uri = "https://www.gettyimages.fr/photos/{0}?assettype=image&excludenudity=false&license=rf&family=creative&phrase={1}&sort=mostpopular&page={2}";
	private readonly Regex RegexCheck = new(@"^(https:\/\/)?s?:?([^\s([""<,>\/]*)(\/)[^\s["",><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s["",><]*)?");

	/// <summary>
	/// GetImagesAsync for Getty
	/// </summary>
	/// <param name="query">string param</param>
	/// <returns></returns>
	public async Task<List<string>> GetImagesAsync(string query, int GettyMaxPage)
	{
		List<string> tmp = new();

		try
		{
			for (int i = 1; i < GettyMaxPage + 1; i++)
			{
				object[] args = new object[] { query, query, i.ToString() };
				HtmlDocument doc = await http.Get(uri, args);
				IEnumerable<string> urls = doc.DocumentNode.Descendants("source").Select(e => e.GetAttributeValue("srcSet", null)).Where(s => !string.IsNullOrEmpty(s));

				if (urls.Count() == 0)
					break;

				for (int j = 0; j < urls.Count(); j++)
				{
					var truc = new Uri(urls.ElementAt(j));
					if (truc == null)
						continue;

					tmp.Add(urls.ElementAt(j).Replace("&amp;", "&"));
				}
			}
		}
		catch (Exception e) { if (e.GetType().Name != "UriFormatException") { }
			if (settings.printErrorLog) { Console.WriteLine("Getty" + e); } }
		return tmp;
	}

	public override async Task<int> GetImages(AsyncCallback ac, params object[] args)
	{
		if (!await redisCheckCount())
			return 0;

		var urls = await GetImagesAsync((string)args[0], (int)args[1]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		var result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
        SettingsDll.nbPushTotal += result;
        if (settings.printLog)
            Console.WriteLine("Getty " + result);

        return (int)result;
    }
}