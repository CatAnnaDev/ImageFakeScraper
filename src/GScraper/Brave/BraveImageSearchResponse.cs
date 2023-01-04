

namespace GScraper.Brave;

internal class BraveImageSearchResponse
{
    [JsonPropertyName("results")]
    public BraveImageResultModel[] Results { get; set; } = null!;
}

internal class BraveImageResultModel : BraveImageResult
{
    [System.Text.Json.Serialization.JsonConstructor]
    public BraveImageResultModel(BraveImageProperties properties)
        : base(properties)
    {
        Properties = null!;
    }

    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonPropertyName("properties")]
    public BraveImageProperties Properties { get; }

}

internal class BraveImageProperties
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;
}
