using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GScraper.Brave;

// TODO: Add support for cancellation tokens and regular search method

/// <summary>
/// Represents a Brave Search scraper.
/// </summary>
public class BraveScraper : IDisposable
{
    /// <summary>
    /// Returns the default API endpoint.
    /// </summary>
    public const string DefaultApiEndpoint = "https://search.brave.com/api/";

    private readonly string _defaultUserAgent = GScraperRandomUa.RandomUserAgent;
    private static readonly Uri _defaultBaseAddress = new(DefaultApiEndpoint);

    private readonly HttpClient _httpClient;
    private readonly List<string> tmp = new();
    private bool _disposed;

    private BraveImageSearchResponse response;

    public BraveScraper() : this(new HttpClient())
    {
    }

    public BraveScraper(HttpClient client)
    {
        _httpClient = client;
        Init(_httpClient, _defaultBaseAddress);
    }

    public BraveScraper(HttpClient client, string apiEndpoint)
    {
        _httpClient = client;
        Init(_httpClient, new Uri(apiEndpoint));
    }

    private void Init(HttpClient client, Uri apiEndpoint)
    {
        GScraperGuards.NotNull(client, nameof(client));
        GScraperGuards.NotNull(apiEndpoint, nameof(apiEndpoint));

        _httpClient.BaseAddress = apiEndpoint;

        try
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);
        }
        catch { _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36"); }
    }


    public async Task<List<string>> GetImagesAsync(string query)
    {
        GScraperGuards.NotNull(query, nameof(query));

        Uri uri = new(BuildImageQuery(query), UriKind.Relative);

        System.IO.Stream stream = await _httpClient.GetStreamAsync(uri).ConfigureAwait(false);
        try
        {
            tmp.Clear();
            response = (await JsonSerializer.DeserializeAsync(stream, BraveImageSearchResponseContext.Default.BraveImageSearchResponse).ConfigureAwait(false))!;
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

    private static string BuildImageQuery(string query)
    {
        string url = $"images?q={Uri.EscapeDataString(query)}&safesearch=Off&size=All&_type=All&layout=All&color=All&license=All&source=web";
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

[JsonSerializable(typeof(BraveImageSearchResponse))]
internal partial class BraveImageSearchResponseContext : JsonSerializerContext
{
}