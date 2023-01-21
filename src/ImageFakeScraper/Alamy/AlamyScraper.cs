using Newtonsoft.Json;
#pragma warning disable CS8602, CS8604, CS8618, CS1634, CS8600
namespace ImageFakeScraper.Alamy
{
	public class AlamyScraper : Scraper
	{
		private const string uri = "https://www.alamy.com/search-api/search/?qt={0}&sortBy=relevant&ispartial=false&type=picture&geo=FR&pn={1}&ps={2}"; // qt query, pn page numb, ps page size

		public async Task<(List<string>, double)> GetImagesAsync(string query, int AlamyMaxPage, int AlamyMaxResult, bool UnlimitedCrawlPage)
		{
			List<string> tmp = new();
			double dlspeedreturn = 0;
			try
			{
				int page = AlamyMaxPage + 1;
				for (int i = 1; i < page; i++)
				{
					string[] args = new string[] { query, i.ToString(), AlamyMaxResult.ToString() };
					(string jsonGet, double dlspeed) = await http.GetJson(uri, args);

					dlspeedreturn += dlspeed;

					Root jsonparsed = JsonConvert.DeserializeObject<Root>(jsonGet);

					if (jsonparsed == null || jsonparsed.Items == null || jsonparsed.Items.Count == 0)
					{
						break;
					}

					for (int j = 0; j < jsonparsed.Items.Count; j++)
					{
						Uri truc = new(jsonparsed.Items[j].Renditions.Comp.Href);
						if (truc == null)
						{
							continue;
						}

						tmp.Add(jsonparsed.Items[j].Renditions.Comp.Href);
					}

					if (UnlimitedCrawlPage)
					{
						page++;
					}
				}

			}
			catch (Exception e) { if (e.GetType().Name != "UriFormatException") { } if (settings.printErrorLog) { Console.WriteLine("Alamy" + e); } }
			return (tmp, dlspeedreturn);
		}

		public override async Task<(int, double)> GetImages(AsyncCallback ac, params object[] args)
		{

			if (!await redisCheckCount())
			{
				return (0, 0);
			}

			(List<string> urls, double dlspeed) = await GetImagesAsync((string)args[0], (int)args[1], (int)args[2], (bool)args[3]);
			RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);

			long result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
			TotalPush += result;
			SettingsDll.nbPushTotal += result;

			if (settings.printLog)
			{
				Console.WriteLine("alamy " + result);
			}

			return ((int)result, dlspeed);
		}
	}
}

