﻿namespace ImageFakeScraper;

public class httpRequest
{
	public async Task<HtmlDocument> Get(string uri, params object[] query)
    {
		HttpClient client = new();
		HtmlDocument doc = new();
		try
        {
			
			string url = string.Format(uri, query);

			HttpResponseMessage resp = await client.GetAsync(url);
            if (resp.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("TooManyRequests (429)");
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
		string url = string.Format(uri, query);
        HttpResponseMessage resp = await client.GetAsync(url);
        string data = await resp.Content.ReadAsStringAsync();
		return data;
    }

    public async Task<string> PostJson(string uri, string json)
    {
		HttpClient client = new();
		StringContent content = new(json, Encoding.UTF8, "application/json");
        HttpResponseMessage result = await client.PostAsync(uri, content);
        string data = await result.Content.ReadAsStringAsync();
		return data;
    }
}
