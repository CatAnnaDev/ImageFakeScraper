namespace ImageFakeScraper.Qwant
{
	public class QwantScraper : Scraper
	{

        private const string uri = "https://api.qwant.com/v3/search/images?q={0}&count=250&offset=0&locale=fr_fr&s=0";

        public async Task<List<string>> GetImagesAsync(string query)
        {
            List<string> tmp = new();
            try
            {
                string[] args = new string[] { query.Replace(" ", "%20") };
                string jsonGet = await http.GetJson(uri, args);
                Root jsonparsed = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(jsonGet);

                if (jsonparsed == null || jsonparsed.data.result == null || jsonparsed.data.result.items.Count == 0)
                    return tmp;

                for (int j = 0; j < jsonparsed.data.result.items.Count; j++)
                {
                    var truc = new Uri(jsonparsed.data.result.items[j].media);
                    if (truc == null)
                        continue;

                    tmp.Add(jsonparsed.data.result.items[j].media);
                }

            }
            catch (Exception e) { if (e.GetType().Name != "UriFormatException") { } Console.WriteLine("Qwant" + e); }
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
                Console.WriteLine("Qwant " + result);

            return (int)result;
        }
    }
}

