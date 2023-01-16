namespace ImageFakeScraper;

public class SettingsDll
{

    public static long nbPushTotal { get; set; } = 0;
    // Alamy
    public int AlamyMaxPage { get; set; }
    public int AlamyPageSize { get; set; }
    public bool AlamyUnlimitedPage { get; set; }

    // Getty
    public int GettyMaxPage { get; set; }

    // Immerse
    public int ImmerseMaxPage { get; set; }
    public int ImmersePageSize { get; set; }

    // OpenVerse
    public int OpenVerseMaxPage { get; set; }

    // EveryPixel
    public int EveryPixelMaxPage { get; set; }
}
