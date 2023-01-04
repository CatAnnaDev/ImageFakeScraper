namespace GScraper;

public class GoogleImageResult : IImageResult
{
    internal GoogleImageResult(GoogleOriginalImage originalImage)
    {
        Url = originalImage.Url;
    }

    public string Url { get; }
    public override string ToString() => Url;
}