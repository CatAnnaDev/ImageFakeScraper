#pragma warning disable CS8602, CS8604, CS8618, CS1634
using System.Diagnostics;

namespace ImageFakeScraper;

public class httpRequest
{
	private readonly SettingsDll settings = new();

	private SimpleMovingAverageLong downloadSpeed;

	public virtual void setMovingAverageDownloadSpeed(SimpleMovingAverageLong movingAverageDL)
	{
		downloadSpeed = movingAverageDL;
	}

	public async Task<(HtmlDocument, double)> Get(string uri, params object[] query)
	{
		HttpClient client = new();
		HtmlDocument doc = new();
		double dlSpeed = 0.0;
		try
		{
			Uri url = new(string.Format(uri, query));
			client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.2 Safari/605.1.15");

			Stopwatch stopwatch = new();
			stopwatch.Start();

			HttpResponseMessage resp = await client.GetAsync(url);
			if (resp.StatusCode == HttpStatusCode.TooManyRequests && settings.printErrorLog)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("TooManyRequests GetAsync (429) " + url);
				Console.ResetColor();
			}
			byte[] bytes = await resp.Content.ReadAsByteArrayAsync();

			stopwatch.Stop();

			dlSpeed += bytes.Length / stopwatch.Elapsed.TotalSeconds;

			try
			{
				SettingsDll.downloadTotal += bytes.Length;
			}
			catch { }
			doc.LoadHtml(Encoding.UTF8.GetString(bytes));
		}
		catch { }
		return (doc, dlSpeed);
	}

	public async Task<(string, double)> GetJson(string uri, params object[] query)
	{
		double dlSpeed = 0.0;
		HttpClient client = new();
		client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.2 Safari/605.1.15");

		string url = string.Format(uri, query);

		Stopwatch stopwatch = new();
		stopwatch.Start();


		HttpResponseMessage resp = await client.GetAsync(url);
		if (resp.StatusCode == HttpStatusCode.TooManyRequests && settings.printErrorLog)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("TooManyRequests GetJson (429) " + url);
			Console.ResetColor();
		}

		byte[] bytes = await resp.Content.ReadAsByteArrayAsync();

		stopwatch.Stop();

		dlSpeed += bytes.Length / stopwatch.Elapsed.TotalSeconds;

		try
		{
			SettingsDll.downloadTotal += bytes.Length;
		}
		catch { }
		return (Encoding.UTF8.GetString(bytes), dlSpeed);
	}

	public async Task<(string, double)> PostJson(string uri, string json)
	{
		double dlSpeed = 0.0;
		HttpClient client = new();
		client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.2 Safari/605.1.15");

		StringContent content = new(json, Encoding.UTF8, "application/json");

		Stopwatch stopwatch = new();
		stopwatch.Start();

		HttpResponseMessage resp = await client.PostAsync(uri, content);
		if (resp.StatusCode == HttpStatusCode.TooManyRequests && settings.printErrorLog)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("TooManyRequests PostJson (429) " + uri);
			Console.ResetColor();
		}

		byte[] bytes = await resp.Content.ReadAsByteArrayAsync();

		stopwatch.Stop();

		dlSpeed += bytes.Length / stopwatch.Elapsed.TotalSeconds;
		try
		{
			SettingsDll.downloadTotal += bytes.Length;
		}
		catch { }
		return (Encoding.UTF8.GetString(bytes), dlSpeed);
	}
}
