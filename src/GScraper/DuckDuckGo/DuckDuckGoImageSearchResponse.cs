namespace ImageFakeScraper.DuckDuckGo;

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

public class DuckDuckGoImageResult : IImageResult
{
    internal DuckDuckGoImageResult(string url)
    {
        Url = url;
    }
    [JsonPropertyName("image")]
    public string Url { get; }

    public override string ToString() => Url;
}
