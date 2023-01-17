namespace ImageFakeScraperExample;

public class Settings
{
    public Settings() { }
    // Google
    public bool GoogleRun { get; set; }

    // UnsplashRun
    public bool UnsplashRun { get; set; }

    // QwantRun
    public bool QwantRun { get; set; }

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
    public bool PrintLog { get; set; }
    public bool PrintLogTag { get; set; }

}
