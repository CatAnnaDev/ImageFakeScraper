namespace ImageFakeScraper;

public class SettingsDll
{
    public SettingsDll() { }
    // Getty
    public int GettyMaxPage { get; set; } = 500;

    // Immerse
    public int ImmerseMaxPage { get; set; } = 2;
    public int ImmersePageSize { get; set; } = 1000;

    // OpenVerse
    public int OpenVerseMaxPage { get; set; } = 2;

    // EveryPixel
    public int EveryPixelMaxPage { get; set; } = 500;
}
