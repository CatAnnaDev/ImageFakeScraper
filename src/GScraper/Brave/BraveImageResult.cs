
namespace GScraper.Brave;

public class BraveImageResult : IImageResult
{
    internal BraveImageResult(BraveImageProperties properties)
    {
        Url = properties.Url;
    }

    public string Url { get; }

    public override string ToString() => Url;
}