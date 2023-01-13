namespace ImageFakeScraperExample;

public class Settings
{
    public Settings() { }
    // Google
    public bool GoogleRun { get; set; }

    // DuckduckGO
    public bool DuckduckGORun { get; set; }

    // Brave
    public bool BraveRun { get; set; }

    // OpenVerse
    public bool OpenVerseRun { get; set; }

    // Alamy
    public bool AlamyRun { get; set; }
    
    // Bing
    public bool BingRun { get; set; }

    // Yahoo
    public bool YahooRun { get; set; }

    // GettyImage
    public bool GettyImageRun { get; set; }

    // Immerse
    public bool ImmerseRun { get; set; }

    // EveryPixel
    public bool EveryPixelRun { get; set; }

    // Redis image push
    public int stopAfter { get; set; }
    public bool useMongoDB { get; set; }
    public bool PrintLog { get; set; }

    // Main 
    public bool PrintLogMain { get; set; }
    public bool GetNewTagGoogle { get; set; }
    public bool GetNewTagBing { get; set; }


}
