#pragma warning disable CS8602, CS8604, CS8618, CS1634
namespace ImageFakeScraper;

public class httpRequest
{
    SettingsDll settings = new();
	public async Task<HtmlDocument> Get(string uri, params object[] query)
    {
		HttpClient client = new();
		HtmlDocument doc = new();
		try
        {
			string url = string.Format(uri, query);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.2 Safari/605.1.15");

            HttpResponseMessage resp = await client.GetAsync(url);
            if (resp.StatusCode == HttpStatusCode.TooManyRequests && settings.printErrorLog)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("TooManyRequests GetAsync (429) " + url);
                Console.ResetColor();
            }
            string data = await resp.Content.ReadAsStringAsync();
            doc.LoadHtml(data);

        }
        catch { }
		return doc;
    }

    public async Task<string> GetJson(string uri, params object[] query)
    {
		HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.2 Safari/605.1.15");

        string url = string.Format(uri, query);
        HttpResponseMessage resp = await client.GetAsync(url);
		if (resp.StatusCode == HttpStatusCode.TooManyRequests && settings.printErrorLog)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("TooManyRequests GetJson (429) " + url);
			Console.ResetColor();
		}
		string data = await resp.Content.ReadAsStringAsync();
		return data;
    }

    public async Task<string> PostJson(string uri, string json)
    {
		HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.2 Safari/605.1.15");

        StringContent content = new(json, Encoding.UTF8, "application/json");
        HttpResponseMessage result = await client.PostAsync(uri, content);
		if (result.StatusCode == HttpStatusCode.TooManyRequests && settings.printErrorLog)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("TooManyRequests PostJson (429) " + uri);
			Console.ResetColor();
		}
		string data = await result.Content.ReadAsStringAsync();
		return data;
    }
}
