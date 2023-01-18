using Newtonsoft.Json;
#pragma warning disable CS8602, CS8604, CS8618, CS1634, CS8600
namespace ImageFakeScraper.Pixel;

public class PixelScraper : Scraper
{
	private const string uri = "https://www.everypixel.com/search/search?q={0}&limit=20000&json=1&page={1}";
	private readonly Regex RegexCheck = new(@"^(http|https:\/\/):?([^\s([<,>\/]*)(\/)[^\s[,><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s[,><]*)?");

	public async Task<List<string>> GetImagesAsync(string query, int EveryPixelMaxPage)
	{
		List<string> tmp = new();

		try
		{

			for (int i = 1; i < EveryPixelMaxPage + 1; i++)
			{
				string[] args = new string[] { query, i.ToString() };
				string jsonGet = await http.GetJson(uri, args);
				Root jsonparsed = JsonConvert.DeserializeObject<Root>(jsonGet);
				if (jsonparsed != null || jsonparsed.images != null || jsonparsed.images.images_0 == null)
					break;

				for (int j = 0; j < jsonparsed.images.images_0.Count; j++)
				{
					var truc = new Uri(jsonparsed.images.images_0[j].url);
					if (truc == null)
						continue;

					tmp.Add(jsonparsed.images.images_0[j].url);

				}
			}
		}
		catch (Exception e) { if (e.GetType().Name != "UriFormatException") { }
			if (settings.printErrorLog) { Console.WriteLine("Pixel" + e); } }
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
            Console.WriteLine("Pixel " + result);

        return (int)result;
    }
}