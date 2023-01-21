using Newtonsoft.Json;
#pragma warning disable CS8602, CS8604, CS8618, CS1634, CS8600
namespace ImageFakeScraper.OpenVerse;

public class OpenVerseScraper : Scraper
{
	private const string uri = "https://api.openverse.engineering/v1/images/?format=json&q={0}&page={1}&mature=false";

	public async Task<(List<string>, double)> GetImagesAsync(string query, int OpenVerseMaxPage)
	{
		List<string> tmp = new();
		double dlspeedreturn = 0;
		try
		{
			int page = OpenVerseMaxPage + 1;
			for (int i = 1; i < page; i++)
			{
				string[] args = new string[] { query, i.ToString() };
				(string jsonGet, double dlspeed) = await http.GetJson(uri, args);
				dlspeedreturn = dlspeed;
				Root jsonparsed = JsonConvert.DeserializeObject<Root>(jsonGet);
				if (jsonparsed == null || jsonparsed.results == null)
				{
					break;
				}

				//page = jsonparsed.page_count;

				for (int j = 0; j < jsonparsed.results.Count; j++)
				{
					Uri truc = new(jsonparsed.results[j].url);
					if (truc == null)
					{
						continue;
					}

					tmp.Add(jsonparsed.results[j].url);

				}
			}
		}
		catch (Exception e)
		{
			if (e.GetType().Name != "UriFormatException") { }
			if (settings.printErrorLog) { Console.WriteLine("Open" + e); }
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
			Console.WriteLine("Open " + result);
		}

		return ((int)result, dlspeed);
	}
}
