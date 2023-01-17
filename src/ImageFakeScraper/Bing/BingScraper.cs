namespace ImageFakeScraper.Bing;


public class BinImageFakeScraper : Scraper
{

	private const string uri = "https://www.bing.com/images/search?q={0}&ghsh=0&ghacc=0&first=1&tsc=ImageHoverTitle&adlt=off";
	private readonly Regex RegexCheck = new(@"^(http|https:):?([^\s([<,>]*)(\/)[^\s[,><]*(\?[^\s[,><]*)?");
	private Queue<string> qword = new Queue<string>();

	// (https:\/\/)?s?:?([^\s(["<,>/]*)(\/)[^\s[",><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s[",><]*)?

	// Regex.Match(url, @"^(?:https?://)?(?:[^@/\n]+@)?(?:www.)?([^:/?\n]+)").Groups[1].Value


	public async Task<List<string>> GetImagesAsync(string query, IDatabase redis)
	{
		List<string> tmp = new();
		try
		{

			string[] args = new string[] { query };
			HtmlDocument doc = await http.Get(uri, args);
			IEnumerable<string> urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("src", null)).Where(s => !string.IsNullOrEmpty(s));

			HtmlNodeCollection tag = doc.DocumentNode.SelectNodes("//span[@class='suggestion-title']");

			if (tag != null)
			{
				foreach (HtmlNode? item in tag)
				{
					redis.SetAdd("words_list", item.FirstChild.InnerText);
					//qword.Enqueue(item.FirstChild.InnerText);
				}
			}

			for (int i = 0; i < urls.Count(); i++)
			{

				string cleanUrl = Regex.Replace(urls.ElementAt(i), @"[?&][^?&]+=[^?&]+", "");
				if (cleanUrl.EndsWith("th") || urls.ElementAt(i).Contains("th?id="))
					continue;

				var truc = new Uri(cleanUrl);
				if (truc == null)
					continue;
				tmp.Add(cleanUrl);
			}
		}
		catch (Exception e) { if (e.GetType().Name != "UriFormatException") { } }
		return tmp;
	}

	public override async Task<int> GetImages(AsyncCallback ac, params object[] args)
	{
		if (!await redisCheckCount())
			return 0;

		var urls = await GetImagesAsync((string)args[0], (IDatabase)args[4]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		var result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
		SettingsDll.nbPushTotal += result;
        if (SettingsDll.printLog)
            Console.WriteLine("Bing " + result);
        return (int)result;
    }
}