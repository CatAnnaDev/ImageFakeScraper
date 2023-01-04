namespace ImageFakeScraper.Google;

public class GoogleScraper
{

    public const string DefaultApiEndpoint = "https://www.google.com/search";

    private string _defaultUserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36";

    private static readonly Uri _defaultBaseAddress = new(DefaultApiEndpoint);

    private readonly HttpClient _httpClient;
    private readonly List<string> tmp = new();

    public static string completUrl = DefaultApiEndpoint;

    public static bool gg { get; set; } = true;

    private GoogleImageResultModel[] images;

    public GoogleScraper() : this(new HttpClient())
    {
    }

    public GoogleScraper(HttpClient client)
    {

        _httpClient = client;
        Init(_httpClient, _defaultBaseAddress);
    }

    [Obsolete("This constructor is deprecated and it will be removed in a future version. Use GoogleScraper(HttpClient) instead.")]
    public GoogleScraper(HttpClient client, string apiEndpoint)
    {
        _httpClient = client;
        Init(_httpClient, new Uri(apiEndpoint));
    }

    private void Init(HttpClient client, Uri apiEndpoint)
    {
        ImageFakeScraperGuards.NotNull(client, nameof(client));
        ImageFakeScraperGuards.NotNull(apiEndpoint, nameof(apiEndpoint));

        _httpClient.BaseAddress = apiEndpoint;

        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);
        }
    }

    public async Task<List<string>> GetImagesAsync(string query)
    {
        ImageFakeScraperGuards.NotNull(query, nameof(query));

        Uri uri = new Uri(BuildImageQuery(query), UriKind.Relative);
        completUrl += uri;

        HttpResponseMessage resp = await _httpClient.GetAsync(uri);
        if (resp.StatusCode == HttpStatusCode.TooManyRequests)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            await Console.Out.WriteLineAsync("Google stopped: TooManyRequests (429)");
            gg = false;
            Console.ResetColor();
        }

        byte[] bytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

        try
        {
            tmp.Clear();
            images = JsonSerializer.Deserialize(bytes.AsSpan(5, bytes.Length - 5), GoogleImageSearchResponseContext.Default.GoogleImageSearchResponse)!.Ischj.Metadata;
            if (images != null)
            {
                foreach (GoogleImageResultModel data in images)
                {
                    tmp.Add(data.Url);
                }
            }
        }
        catch { return null; }

        return tmp;
    }

    private static string BuildImageQuery(string query)
    {
        string url = $"?q={Uri.EscapeDataString(query)}&tbm=isch&asearch=isch&async=_fmt:json,p:2&tbs=&safe=off";

        return url;
    }
}

[JsonSerializable(typeof(GoogleImageSearchResponse))]
internal partial class GoogleImageSearchResponseContext : JsonSerializerContext
{
}