using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;

namespace GScraper.Brave;

internal class BraveImageSearchResponse
{
    [JsonPropertyName("results")]
    public BraveImageResultModel[] Results { get; set; } = null!;
}

internal class BraveImageResultModel : BraveImageResult
{
    [JsonConstructor]
    public BraveImageResultModel(BraveImageProperties properties)
        : base(properties)
    {
        Properties = null!;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonPropertyName("properties")]
    public BraveImageProperties Properties { get; }

}

internal class BraveImageProperties
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;
}
