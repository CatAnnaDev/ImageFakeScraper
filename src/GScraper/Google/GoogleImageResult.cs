using GScraper.Google;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;

namespace GScraper; // TODO: Fix namespace

public class GoogleImageResult : IImageResult
{
    internal GoogleImageResult(GoogleOriginalImage originalImage)
    {
        Url = originalImage.Url;
    }

    public string Url { get; }
    public override string ToString() => Url;
}