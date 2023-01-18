namespace ImageFakeScraper;
#pragma warning disable
internal static class ImageFakeScraperExtensions
{
    public static JsonElement Last(in this JsonElement element)
    {
        int length = element.GetArrayLength();
        return element[length - 1];
    }
}