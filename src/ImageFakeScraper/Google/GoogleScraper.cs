using Newtonsoft.Json;
using StackExchange.Redis;

namespace ImageFakeScraper.Google;

public class GoogleScraper : Scraper
{
	private const string uri = "https://www.google.com/search?q={0}&tbm=isch&asearch=isch&async=_fmt:json,p:2&tbs=&safe=off";

	public GoogleScraper(IDatabase redis, Dictionary<string, object> key) : base(redis, key) { }

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
				if (metadata.original_image.url != null)
					tmp.Add(metadata.original_image.url);
			}

		}catch(Exception e) { }

		return tmp;
    }

	public override async void GetImages(AsyncCallback ac, params object[] args)
	{
	if (!await redisCheckCount())
		return;

		var urls =await GetImagesAsync((string)args[0]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		var result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
		SettingsDll.nbPushTotal += result;
		Console.WriteLine("Google " + result);
	}
}
