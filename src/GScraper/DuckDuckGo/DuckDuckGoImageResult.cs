namespace GScraper.DuckDuckGo;


[DebuggerDisplay("Title: {Title}, Url: {Url}")]
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
