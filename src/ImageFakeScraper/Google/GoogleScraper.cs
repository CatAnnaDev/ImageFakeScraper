using Newtonsoft.Json;
#pragma warning disable CS8602, CS8604, CS8618, CS1634, CS8600
namespace ImageFakeScraper.Google;

public class GoogleScraper : Scraper
{
	private const string uri = "https://www.google.com/search?q={0}&tbm=isch&asearch=isch&async=_fmt:json,p:2&tbs=&safe=off";

	public async Task<(List<string>, double)> GetImagesAsync(string query)
	{
		List<string> tmp = new();
		double dlspeedreturn = 0;
		try
		{
			string[] args = new string[] { query };
			(string jsonGet, double dlspeed) = await http.GetJson(uri, args);
			dlspeedreturn = dlspeed;
			string jspnUpdate = jsonGet[5..];

			Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(jspnUpdate);
			foreach (Metadata metadata in myDeserializedClass.ischj.metadata)
			{
				Uri truc = new(metadata.original_image.url);
				if (truc == null)
				{
					continue;
				}

				tmp.Add(metadata.original_image.url);
			}

		}
		catch (Exception e)
		{
			if (e.GetType().Name != "UriFormatException") { }
			if (settings.printErrorLog) { Console.WriteLine("Google" + e); }
		}

		return (tmp, dlspeedreturn);
	}

	public override async Task<(int, double)> GetImages(AsyncCallback ac, params object[] args)
	{
		if (!await redisCheckCount())
		{
			return (0, 0);
		}

		(List<string> urls, double dlspeed) = await GetImagesAsync((string)args[0]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		long result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
		SettingsDll.TotalPushGoogle += result;
		SettingsDll.nbPushTotal += result;
		if (settings.printLog)
		{
			Console.WriteLine("Google " + result);
		}

		return ((int)result, dlspeed);
	}
}
