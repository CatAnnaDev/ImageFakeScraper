using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFakeScraper.Shutterstock
{
	public class ShutterstockScraper : Scraper
	{
		private const string uri = "https://www.shutterstock.com/_next/data/k8Ph_CBStzwUa7WhxQwok/en/_shutterstock/search/{0}.json";

		public async Task<(List<string>, double)> GetImagesAsync(string query)
		{
			List<string> tmp = new();
			double dlspeedreturn = 0;
			try
			{
				string[] args = new string[] { query };
				(dynamic jsonGet, double dlspeed) = await http.GetJson(uri, args);
				dlspeedreturn = dlspeed;

				Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(jsonGet);

				if (myDeserializedClass == null || myDeserializedClass.pageProps == null || myDeserializedClass.pageProps.assets.Count == 0)
				{
					return (tmp, 0);
				}

				for (int i = 0; i < myDeserializedClass.pageProps.assets.Count; i++)
				{
					Uri truc = new(myDeserializedClass.pageProps.assets[i].src);
					if (truc == null)
					{
						continue;
					}

					tmp.Add(myDeserializedClass.pageProps.assets[i].src);
				}
			}
			catch (Exception e)
			{
				if (e.GetType().Name != "UriFormatException") { }
				if (settings.printErrorLog) { Console.WriteLine("Shutterstock" + e); }
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
			SettingsDll.TotalPushShutterstock += result;
			SettingsDll.nbPushTotal += result;
			if (settings.printLog)
			{
				Console.WriteLine("Shutterstock " + result);
			}

			return ((int)result, dlspeed);
		}
	}
}
