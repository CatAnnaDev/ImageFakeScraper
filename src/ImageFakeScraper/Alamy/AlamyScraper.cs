using Newtonsoft.Json;

namespace ImageFakeScraper.Alamy
{
	public class AlamyScraper : Scraper
	{
		private const string uri = "https://www.alamy.com/search-api/search/?qt={0}&sortBy=relevant&ispartial=false&type=picture&geo=FR&pn={1}&ps={2}"; // qt query, pn page numb, ps page size

		public async Task<List<string>> GetImagesAsync(string query, int AlamyMaxPage, int AlamyMaxResult, bool UnlimitedCrawlPage)
		{
			List<string> tmp = new();
			try
			{

				int page = AlamyMaxPage + 1;
				for (int i = 1; i < page; i++)
				{
					string[] args = new string[] { query, i.ToString(), AlamyMaxResult.ToString() };
					string jsonGet = await http.GetJson(uri, args);
					Root jsonparsed = JsonConvert.DeserializeObject<Root>(jsonGet);

					if (jsonparsed == null || jsonparsed.Items == null || jsonparsed.Items.Count == 0)
						break;

					for (int j = 0; j < jsonparsed.Items.Count; j++)
					{
						var truc = new Uri(jsonparsed.Items[j].Renditions.Comp.Href);
						if (truc == null)
							continue;

						tmp.Add(jsonparsed.Items[j].Renditions.Comp.Href);
					}

					if (UnlimitedCrawlPage)
						page++;
				}

			}
			catch (Exception e) { if (e.GetType().Name != "UriFormatException") { } Console.WriteLine("Alamy" + e); }
			return tmp;
		}

		public override async Task<int> GetImages(AsyncCallback ac, params object[] args)
		{

			if (!await redisCheckCount())
				return 0;

			var urls = await GetImagesAsync((string)args[0], (int)args[1], (int)args[2], (bool)args[3]);
			RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);

			var result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
			SettingsDll.nbPushTotal += result;

            if (SettingsDll.printLog)
				Console.WriteLine("alamy " + result);

			return (int)result;
        }
	}
}

