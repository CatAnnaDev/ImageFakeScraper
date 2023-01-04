
namespace GScraper.DuckDuckGo;

// fix duckduck ? ( en vrai balek ) 
public class DuckDuckGoScraper : IDisposable
{
    public const string DefaultApiEndpoint = "https://duckduckgo.com";

    public const int MaxQueryLength = 500;

    private static ReadOnlySpan<byte> TokenStart => new[] { (byte)'v', (byte)'q', (byte)'d', (byte)'=', (byte)'\'' };

    private string _defaultUserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36";

    private static readonly Uri _defaultBaseAddress = new(DefaultApiEndpoint);

    private readonly HttpClient _httpClient;
    private readonly List<string> tmp = new();

    private bool _disposed;

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
        GScraperGuards.NotNull(client, nameof(client));
        GScraperGuards.NotNull(apiEndpoint, nameof(apiEndpoint));

        _httpClient.BaseAddress = apiEndpoint;

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);

        _httpClient.DefaultRequestHeaders.Referrer ??= _httpClient.BaseAddress;
    }

    public async Task<List<string>?> GetImagesAsync(string query)
    {
        GScraperGuards.NotNull(query, nameof(query));
        GScraperGuards.ArgumentInRange(query.Length, MaxQueryLength, nameof(query), $"The query cannot be larger than {MaxQueryLength}.");

        string token = await GetTokenAsync(query).ConfigureAwait(false);
        Uri uri = new(BuildImageQuery(token, query), UriKind.Relative);

        Stream stream = await _httpClient.GetStreamAsync(uri).ConfigureAwait(false);
        try
        {
            tmp.Clear();
            response = (await System.Text.Json.JsonSerializer.DeserializeAsync(stream, DuckDuckGoImageSearchResponseContext.Default.DuckDuckGoImageSearchResponse).ConfigureAwait(false))!;
            if (response != null)
            {
                foreach (var data in response.Results)
                {
                    tmp.Add(data.Url);
                }
            }
        }
        catch { return null; }
        return tmp;
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
            throw new GScraperException("Failed to get the DuckDuckGo token.", "DuckDuckGo");
        }

        ReadOnlySpan<byte> sliced = rawHtml[(startIndex + TokenStart.Length)..];
        int endIndex = sliced.IndexOf((byte)'\'');

        return endIndex == -1
            ? throw new GScraperException("Failed to get the DuckDuckGo token.", "DuckDuckGo")
            : Encoding.UTF8.GetString(sliced[..endIndex].ToArray());
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _httpClient.Dispose();
        }

        _disposed = true;
    }
}

[JsonSerializable(typeof(DuckDuckGoImageSearchResponse))]
internal partial class DuckDuckGoImageSearchResponseContext : JsonSerializerContext
{
}