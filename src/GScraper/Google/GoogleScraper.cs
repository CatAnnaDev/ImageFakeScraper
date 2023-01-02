using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GScraper.Google;

// TODO: Add support for cancellation tokens and regular search method

/// <summary>
/// Represents a Google Search scraper.
/// </summary>
public class GoogleScraper : IDisposable
{
    /// <summary>
    /// Returns the default API endpoint.
    /// </summary>
    public const string DefaultApiEndpoint = "https://www.google.com/search";

    private string _defaultUserAgent = GScraperRandomUa.RandomUserAgent;

    private static readonly Uri _defaultBaseAddress = new(DefaultApiEndpoint);

    private readonly HttpClient _httpClient;
    private readonly List<string> tmp = new();
    private bool _disposed;

    public static string completUrl = DefaultApiEndpoint;

    public static bool gg { get; set; } = true;

    private GoogleImageResultModel[] images;

    /// <summary>
    /// Initializes a new instance of the <see cref="YandexScraper"/> class.
    /// </summary>
    public GoogleScraper() : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YandexScraper"/> class using the provided <see cref="HttpClient"/>.
    /// </summary>
    public GoogleScraper(HttpClient client)
    {

        _httpClient = client;
        Init(_httpClient, _defaultBaseAddress);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YandexScraper"/> class using the provided <see cref="HttpClient"/> and API endpoint.
    /// </summary>
    [Obsolete("This constructor is deprecated and it will be removed in a future version. Use GoogleScraper(HttpClient) instead.")]
    public GoogleScraper(HttpClient client, string apiEndpoint)
    {
        _httpClient = client;
        Init(_httpClient, new Uri(apiEndpoint));
    }

    private void Init(HttpClient client, Uri apiEndpoint)
    {
        GScraperGuards.NotNull(client, nameof(client));
        GScraperGuards.NotNull(apiEndpoint, nameof(apiEndpoint));

        _httpClient.BaseAddress = apiEndpoint;
        //_httpClient.Timeout = TimeSpan.FromSeconds(5);

        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);
            }
            catch { _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36"); }
        }
    }

    /// <summary>
    /// Gets images from Google Images.
    /// </summary>
    /// <remarks>This method returns at most 100 image results.</remarks>
    /// <param name="query">The search query.</param>
    /// <param name="safeSearch">The safe search level.</param>
    /// <param name="size">The image size.</param>
    /// <param name="color">The image color. <see cref="GoogleImageColors"/> contains the colors that can be used here.</param>
    /// <param name="type">The image type.</param>
    /// <param name="time">The image time.</param>
    /// <param name="license">The image license. <see cref="GoogleImageLicenses"/> contains the licenses that can be used here.</param>
    /// <param name="language">The language code to use. <see cref="GoogleLanguages"/> contains the language codes that can be used here.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IEnumerable{T}"/> of <see cref="GScraper.GoogleImageResult"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is null or empty.</exception>
    /// <exception cref="GScraperException">An error occurred during the scraping process.</exception>
    public async Task<List<string>> GetImagesAsync(string query, SafeSearchLevel safeSearch = SafeSearchLevel.Off, GoogleImageSize size = GoogleImageSize.Any,
        string? color = null, GoogleImageType type = GoogleImageType.Any, GoogleImageTime time = GoogleImageTime.Any,
        string? license = null, string? language = null)
    {
        // TODO: Use pagination
        GScraperGuards.NotNull(query, nameof(query));

        var uri = new Uri(BuildImageQuery(query, safeSearch, size, color, type, time, license, language), UriKind.Relative);
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
                foreach (var data in images)
                {
                    tmp.Add(data.Url);
                }
            }
        }
        catch { return null; }

        return tmp;
    }

    private static string BuildImageQuery(string query, SafeSearchLevel safeSearch, GoogleImageSize size, string? color,
        GoogleImageType type, GoogleImageTime time, string? license, string? language)
    {
        string url = $"?q={Uri.EscapeDataString(query)}&tbm=isch&asearch=isch&async=_fmt:json,p:2&tbs=";

        url += size == GoogleImageSize.Any ? ',' : $"isz:{(char)size},";
        url += string.IsNullOrEmpty(color) ? ',' : $"ic:{color},";
        url += type == GoogleImageType.Any ? ',' : $"itp:{type.ToString().ToLowerInvariant()},";
        url += time == GoogleImageTime.Any ? ',' : $"qdr:{(char)time},";
        url += string.IsNullOrEmpty(license) ? "" : $"il:{license}";

        url += "&safe=" + safeSearch switch
        {
            SafeSearchLevel.Off => "off",
            _ => "active"
        };

        if (!string.IsNullOrEmpty(language))
            url += $"&lr=lang_{language}&hl={language}";

        return url;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Dispose()"/>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
            _httpClient.Dispose();

        _disposed = true;
    }
}