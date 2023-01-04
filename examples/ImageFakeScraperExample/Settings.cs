namespace ImageFakeScraperExample;

internal class Settings
{
    // Google
    public static bool GoogleRun { get; set; } = true;
    public static bool GoogleGetNewTag { get; set; } = false;

    // DuckduckGO
    public static bool DuckduckGORun { get; set; } = true;

    // Brave
    public static bool BraveRun { get; set; } = true;

    // OpenVerse
    public static bool OpenVerseRun { get; set; } = true;

    // Bing
    public static bool BingRun { get; set; } = true;
    public static bool BingGetNewTag { get; set; } = false;

    // Yahoo
    public static bool YahooRun { get; set; } = true;

    // GettyImage
    public static bool GettyImageRun { get; set; } = true;

    // Immerse
    public static bool ImmerseRun { get; set; } = true;

    // EveryPixel
    public static bool EveryPixelRun { get; set; } = true;

    // Redis image push
    public static int stopAfter { get; } = 11;
    public static int restartAfter { get; set; } = 10;
    public static bool PrintLog { get; set; } = true;

    // Main 
    public static bool PrintLogMain { get; set; } = true;

}
