namespace ImageFakeScraperExample;

internal class Settings
{
    public Settings() { }
    // Google
    public bool GoogleRun { get; set; } = true;

    // DuckduckGO
    public bool DuckduckGORun { get; set; } = false;

    // Brave
    public bool BraveRun { get; set; } = false;

    // OpenVerse
    public bool OpenVerseRun { get; set; } = true;

    // Bing
    public bool BingRun { get; set; } = true;

    // Yahoo
    public bool YahooRun { get; set; } = true;

    // GettyImage
    public bool GettyImageRun { get; set; } = true;

    // Immerse
    public bool ImmerseRun { get; set; } = true;

    // EveryPixel
    public bool EveryPixelRun { get; set; } = true;

    // Redis image push
    public int stopAfter { get; set; } = 8_000_000;
    public bool useMongoDB { get; set; } = false;
    public bool PrintLog { get; set; } = true;

    // Main 
    public bool PrintLogMain { get; set; } = true;
    public bool GetNewTagGoogle { get; set; } = false;
    public bool GetNewTagBing { get; set; } = false;


}
