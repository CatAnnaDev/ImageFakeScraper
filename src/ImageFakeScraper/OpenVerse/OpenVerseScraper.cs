using Newtonsoft.Json;
namespace ImageFakeScraper.OpenVerse;

public class OpenVerseScraper : Scraper
{

	private SettingsDll settingsDll = new();

	private const string uri = "https://api.openverse.engineering/v1/images/?format=json&q={0}&page={1}&mature=true";
	private readonly Regex RegexCheck = new(@"^(http|https://):?([^\s([<,>/]*)(\/)[^\s[,><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s[,><]*)?");

	public async Task<List<string>> GetImagesAsync(string query, int OpenVerseMaxPage)
	{
		List<string> tmp = new();
		try
		{

			int page = OpenVerseMaxPage + 1;
			for (int i = 1; i < page; i++)
			{
				string[] args = new string[] { query, i.ToString() };
				string jsonGet = await http.GetJson(uri, args);
				Root jsonparsed = JsonConvert.DeserializeObject<Root>(jsonGet);
				if (jsonparsed == null || jsonparsed.results == null)
					break;

				page = jsonparsed.page_count;

				for (int j = 0; j < jsonparsed.results.Count; j++)
				{
					var truc = new Uri(jsonparsed.results[j].url);
					if (truc == null)
						continue;

					tmp.Add(jsonparsed.results[j].url);

				}

			}
		}
		catch (Exception e) { if (e.GetType().Name != "UriFormatException") { } }
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
        if (SettingsDll.printLog)
            Console.WriteLine("Open " + result);

        return (int)result;
    }
}
