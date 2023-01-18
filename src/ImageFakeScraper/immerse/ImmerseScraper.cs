namespace ImageFakeScraper.immerse;

#pragma warning disable CS8602, CS8604, CS8618, CS1634, CS8600, IL2026
public class ImmerseScraper : Scraper
{

	private const string uri = "https://www.immerse.zone/api/immerse/search";
	private readonly Regex RegexCheck = new(@"^(http|https:):?([^\s([<,>]*)(\/)[^\s[,><]*(\?[^\s[,><]*)?");

	public async Task<(List<string>, double)> GetImagesAsync(string query, int pageSize, int ImmerseMaxPage)
	{
		List<string> tmp = new();
		double dlspeedreturn = 0;
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
				(string doc, double dlspeed) = await http.PostJson(uri, jsonString);
				dlspeedreturn = dlspeed;
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
						{
							break;
						}
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
		}
		catch (Exception e)
		{
			if (e.GetType().Name != "UriFormatException") { }
			if (settings.printErrorLog) { Console.WriteLine("Immerse" + e); }
		}
		return (tmp, dlspeedreturn);
	}

	public override async Task<(int, double)> GetImages(AsyncCallback ac, params object[] args)
	{
		if (!await redisCheckCount())
		{
			return (0, 0);
		}

		(List<string> urls, double dlspeed) = await GetImagesAsync((string)args[0], (int)args[1], (int)args[2]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		long result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
		SettingsDll.nbPushTotal += result;
		if (settings.printLog)
		{
			Console.WriteLine("Immerse " + result);
		}

		return ((int)result, dlspeed);
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