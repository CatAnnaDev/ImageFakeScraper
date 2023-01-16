using Newtonsoft.Json;
namespace ImageFakeScraper.Pixel;

public class PixelScraper : Scraper
{
	public PixelScraper(IDatabase redis, Dictionary<string, object> key) : base(redis, key) { }

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
		catch (Exception e) { if (e.GetType().Name != "UriFormatException") Console.WriteLine("Pixel " + e); }
		return tmp;
	}

	public override async void GetImages(AsyncCallback ac, params object[] args)
	{
		if (!await redisCheckCount())
			return;

		var urls = await GetImagesAsync((string)args[0], (int)args[1]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		var result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
		SettingsDll.nbPushTotal += result;
		Console.WriteLine("Pixel " + result);
	}
}