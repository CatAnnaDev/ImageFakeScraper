using Newtonsoft.Json;

namespace ImageFakeScraper.Alamy
{
	public class AlamyScraper : Scraper
	{
		public AlamyScraper(IDatabase redis, Dictionary<string, object> key) : base(redis, key) { }


		private const string uri = "https://www.alamy.com/search-api/search/?qt={0}&sortBy=relevant&ispartial=false&type=picture&geo=FR&pn={1}&ps={2}"; // qt query, pn page numb, ps page size
		private readonly Regex RegexCheck = new(@"^(https:\/\/)?s?:?([^\s([""<,>\/]*)(\/)[^\s["",><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s["",><]*)?");

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
					if (jsonparsed != null)
					{
						if (jsonparsed.Items != null)
						{
							if (jsonparsed.Items.Count != 0)
							{
								for (int j = 0; j < jsonparsed.Items.Count; j++)
								{
									if (RegexCheck.IsMatch(jsonparsed.Items[j].Renditions.Comp.Href))
									{
										tmp.Add(jsonparsed.Items[j].Renditions.Comp.Href);
									}
								}
								if (UnlimitedCrawlPage)
									page++;
							}
							else
								break;
						}
					}
					else
						break;
				}

			}
			catch (Exception e) { Console.WriteLine("Alamy " + e); }
			return tmp;
		}

		public override async void GetImages(AsyncCallback ac, params object[] args)
		{

			if (!await redisCheckCount()) 
				return;

			var urls = await GetImagesAsync((string)args[0], (int)args[1], (int)args[2], (bool)args[3]);
			RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);

			var result = await redis.SetAddAsync(Options["redis_push_key"].ToString(), push);
			SettingsDll.nbPushTotal += result;
			Console.WriteLine("alamy " + result);
		}
	}
}

