﻿namespace ImageFakeScraper.Yahoo;
public class YahooScraper : Scraper
{

    public YahooScraper(IDatabase redis, string key) : base(redis, key) { }

	private const string uri = "https://images.search.yahoo.com/search/images?ei=UTF-8&p={0}&fr2=p%3As%2Cv%3Ai&.bcrumb=4N2SA8f4BZT&save=0";
    private readonly Regex RegexCheck = new(@"^(http|https:):?([^\s([<,>]*)(\/)[^\s[,><]*(\?[^\s[,><]*)?");

    public async Task<List<string>> GetImagesAsync(string query)
    {
		List<string> tmp = new();

		try
		{
            string[] args = new string[] { query };
            HtmlDocument doc = await http.Get(uri, args);
            IEnumerable<string> urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("data-src", null)).Where(s => !string.IsNullOrEmpty(s));

            if (urls != null)
            {

                for (int i = 0; i < urls.Count(); i++)
                {
                    if (RegexCheck.IsMatch(urls.ElementAt(i)))
                    {
                        string cleanUrl = Regex.Replace(urls.ElementAt(i), @"&pid=Api&P=0&w=300&h=300", "");
                        if (!cleanUrl.EndsWith("th") && !urls.ElementAt(i).Contains("th?id="))
                        {
                            tmp.Add(cleanUrl);
                        }
                    }
                }
            }
        }
        catch (Exception e) { Console.WriteLine("Yahoo " + e); }
        return tmp;
    }

	public override async void GetImages(AsyncCallback ac, params object[] args)
	{
		var urls = await GetImagesAsync((string)args[0]);
		RedisValue[] push = Array.ConvertAll(urls.ToArray(), item => (RedisValue)item);
		var result = await redis.SetAddAsync(RedisPushKey, push);
		SettingsDll.nbPushTotal += result;
		Console.WriteLine("Yahoo " + result);
	}
}