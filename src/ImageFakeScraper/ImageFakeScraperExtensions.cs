namespace ImageFakeScraper;
#pragma warning disable CS8602, CS8604, CS8618, CS1634
internal static class ImageFakeScraperExtensions
{
    public static JsonElement Last(in this JsonElement element)
    {
        int length = element.GetArrayLength();
        return element[length - 1];
    }
}