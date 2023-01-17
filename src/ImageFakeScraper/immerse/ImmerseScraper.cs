namespace ImageFakeScraper.immerse;


public class ImmerseScraper : Scraper
{

	private const string uri = "https://www.immerse.zone/api/immerse/search";
	private readonly Regex RegexCheck = new(@"^(http|https:):?([^\s([<,>]*)(\/)[^\s[,><]*(\?[^\s[,><]*)?");

	public async Task<List<string>> GetImagesAsync(string query, int pageSize, int ImmerseMaxPage)
	{
		List<string> tmp = new();

		try
		{
			for (int i = 1; i < ImmerseMaxPage + 1; i++)
			{
				JsonCreatePush json = new()
				{
					searchText = query,
					pageNum = i,
					pageSize = pageSize
				};

				string jsonString = JsonSerializer.Serialize(json);
				string doc = await http.PostJson(uri, jsonString);
				Root jsonparsed = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(doc);

				if (jsonparsed != null)
				{
					if (jsonparsed.data != null)
					{
						if (jsonparsed.data.imageData != null)
						{
							for (int j = 0; j < jsonparsed.data.imageData.Count; j++)
							{
								if (!jsonparsed.data.imageData[j].sourceImageUrl.Contains("images.unsplash.com"))
								{
									tmp.Add(jsonparsed.data.imageData[j].sourceImageUrl);
								}
								//else
								//{
								//	string cleanUrl = Regex.Replace(jsonparsed.data.imageData[j].sourceImageUrl, @"[?&][^?&]+=[^?&]+", "");
								//	tmp.Add(cleanUrl);
								//}
							}
						}
						else
							break;
					}
					else
						break;
				}
				else
					break;
			}
		}
		catch (Exception e) { if (e.GetType().Name != "UriFormatException") { }
			if (settings.printErrorLog) { Console.WriteLine("Immerse" + e); } }
		return tmp;
	}

	public override async Task<int> GetImages(AsyncCallback ac, params object[] args)
	{
		if (!await redisCheckCount())
			return 0;

		var urls = await GetImagesAsync((string)args[0], (int)args[1], (int)args[2]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		var result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
        SettingsDll.nbPushTotal += result;
        if (settings.printLog)
            Console.WriteLine("Immerse " + result);

        return (int)result;
    }
}

public class JsonCreatePush
{

	public string? searchText { get; set; }
	public string imageUrl { get; set; } = "";
	public int? pageNum { get; set; } = 1;
	public int? pageSize { get; set; }
	public string searchType { get; set; } = "image";
}