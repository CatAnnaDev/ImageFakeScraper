namespace ImageFakeScraper.Google;

internal class GoogleImageSearchResponse
{
    [JsonPropertyName("ischj")]
    public Ischj Ischj { get; set; } = null!;
}

internal class Ischj
{
    [JsonPropertyName("metadata")]
    public GoogleImageResultModel[]? Metadata { get; set; }
}

internal class GoogleImageResultModel : GoogleImageResult
{
    [JsonConstructor]
    public GoogleImageResultModel(GoogleOriginalImage originalImage)
        : base(originalImage)
    {
        OriginalImage = null!;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonPropertyName("original_image")]
    public GoogleOriginalImage OriginalImage { get; }

}

internal class GoogleOriginalImage
{

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;
}

public class GoogleImageResult : IImageResult
{
    internal GoogleImageResult(GoogleOriginalImage originalImage)
    {
        Url = originalImage.Url;
    }

    public string Url { get; }
    public override string ToString() => Url;
}
