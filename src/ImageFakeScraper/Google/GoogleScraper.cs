using Newtonsoft.Json;
using StackExchange.Redis;

namespace ImageFakeScraper.Google;

public class GoogleScraper : Scraper
{
	private const string uri = "https://www.google.com/search?q={0}&tbm=isch&asearch=isch&async=_fmt:json,p:2&tbs=&safe=off";

	public async Task<List<string>?> GetImagesAsync(string query)
	{
		List<string> tmp = new();
		try
		{
			string[] args = new string[] { query };
			string jsonGet = await http.GetJson(uri, args);
			var jspnUpdate = jsonGet.Substring(5);

			Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(jspnUpdate);
			foreach (var metadata in myDeserializedClass.ischj.metadata)
			{
				var truc = new Uri(metadata.original_image.url);
				if (truc == null)
					continue;

				tmp.Add(metadata.original_image.url);
			}

		}
		catch (Exception e) { if (e.GetType().Name != "UriFormatException") { } Console.WriteLine("Google" + e); }

		return tmp;
	}

	public override async Task<int> GetImages(AsyncCallback ac, params object[] args)
	{
		if (!await redisCheckCount())
			return 0;

		var urls = await GetImagesAsync((string)args[0]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		var result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
		SettingsDll.nbPushTotal += result;
        if (SettingsDll.printLog)
            Console.WriteLine("Google " + result);

        return (int)result;
    }
}
