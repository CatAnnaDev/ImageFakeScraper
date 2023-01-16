
namespace ImageFakeScraper.DuckDuckGo;

public class DuckDuckGoScraper : Scraper
{
    public const string DefaultApiEndpoint = "https://duckduckgo.com";

    public const int MaxQueryLength = 500;

    private static ReadOnlySpan<byte> TokenStart => new[] { (byte)'v', (byte)'q', (byte)'d', (byte)'=', (byte)'\'' };

    private readonly string _defaultUserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36";

    private static readonly Uri _defaultBaseAddress = new(DefaultApiEndpoint);

    private readonly HttpClient _httpClient;
    private readonly List<string> tmp = new();

    private DuckDuckGoImageSearchResponse response;

    public DuckDuckGoScraper() : this(new HttpClient())
    {
    }

    public DuckDuckGoScraper(HttpClient client)
    {
        _httpClient = client;
        Init(_httpClient, _defaultBaseAddress);
    }

    [Obsolete("This constructor is deprecated and it will be removed in a future version. Use DuckDuckGoScraper(HttpClient) instead.")]
    public DuckDuckGoScraper(HttpClient client, string apiEndpoint)
    {
        _httpClient = client;
        Init(_httpClient, new Uri(apiEndpoint));
    }

    private void Init(HttpClient client, Uri apiEndpoint)
    {
        ImageFakeScraperGuards.NotNull(client, nameof(client));
        ImageFakeScraperGuards.NotNull(apiEndpoint, nameof(apiEndpoint));

        _httpClient.BaseAddress = apiEndpoint;

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);

        _httpClient.DefaultRequestHeaders.Referrer ??= _httpClient.BaseAddress;
    }

    public async Task<List<string>?> GetImagesAsync(string query)
    {
        ImageFakeScraperGuards.NotNull(query, nameof(query));
        ImageFakeScraperGuards.ArgumentInRange(query.Length, MaxQueryLength, nameof(query), $"The query cannot be larger than {MaxQueryLength}.");


        try
        {
            string token = await GetTokenAsync(query).ConfigureAwait(false);
            Uri uri = new(BuildImageQuery(token, query), UriKind.Relative);

            Stream stream = await _httpClient.GetStreamAsync(uri).ConfigureAwait(false);
            tmp.Clear();
            response = (await JsonSerializer.DeserializeAsync(stream, DuckDuckGoImageSearchResponseContext.Default.DuckDuckGoImageSearchResponse).ConfigureAwait(false))!;
            if (response != null)
            {
                for (int i = 0; i < response.Results.Count(); i++)
                {
                    tmp.Add(response.Results[i].Url);
                }
            }
        }
        catch(Exception e) { Console.WriteLine("Duck " + e); return null; }
        return tmp;
    }

    public override async Task<List<string>> GetImages(params object[] args)
    {
        return await GetImagesAsync((string)args[0]);
    }

    private static string BuildImageQuery(string token, string query)
    {
        string url = $"i.js?l=fr-fr&o=json&q={Uri.EscapeDataString(query)}&vqd={token}&f=&p=-1";
        return url;
    }

    private async Task<string?> GetTokenAsync(string query)
    {
        try
        {
            byte[] bytes = await _httpClient.GetByteArrayAsync(new Uri($"?q={Uri.EscapeDataString(query)}", UriKind.Relative)).ConfigureAwait(false);
            return GetToken(bytes);
        }
        catch { Console.WriteLine("Failed to get the DuckDuckGo token."); return null; }
    }

    private static string GetToken(ReadOnlySpan<byte> rawHtml)
    {
        int startIndex = rawHtml.IndexOf(TokenStart);

        if (startIndex == -1)
        {
            throw new ImageFakeScraperException("Failed to get the DuckDuckGo token.", "DuckDuckGo");
        }

        ReadOnlySpan<byte> sliced = rawHtml[(startIndex + TokenStart.Length)..];
        int endIndex = sliced.IndexOf((byte)'\'');

        return endIndex == -1
            ? throw new ImageFakeScraperException("Failed to get the DuckDuckGo token.", "DuckDuckGo")
            : Encoding.UTF8.GetString(sliced[..endIndex].ToArray());
    }
}

[JsonSerializable(typeof(DuckDuckGoImageSearchResponse))]
internal partial class DuckDuckGoImageSearchResponseContext : JsonSerializerContext
{
}