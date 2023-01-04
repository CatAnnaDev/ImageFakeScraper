using System.Text.Json.Serialization;

namespace GScraper.DuckDuckGo;

internal class DuckDuckGoImageSearchResponse
{
    [JsonPropertyName("results")]
    public DuckDuckGoImageResultModel[] Results { get; set; } = null!;
}

internal sealed class DuckDuckGoImageResultModel : DuckDuckGoImageResult
{
    [JsonConstructor]
    public DuckDuckGoImageResultModel(string url) : base(url)
    {
    }
}