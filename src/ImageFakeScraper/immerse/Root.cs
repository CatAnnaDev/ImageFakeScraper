namespace ImageFakeScraper.immerse;
#pragma warning disable CS8602, CS8604, CS8618, CS1634
public class Data
{
    public List<ImageDatum>? imageData { get; set; }
}

public class ImageDatum
{
    public int? id { get; set; }
    public string? imageUrl { get; set; }
    public string? sourceImageUrl { get; set; }
    public object? imageId { get; set; }
    public int? width { get; set; }
    public int? height { get; set; }
    public int? size { get; set; }
    public string? sourceUrl { get; set; }
    public string? authorName { get; set; }
    public string? authorUrl { get; set; }
    public int? sourceResolution { get; set; }
    public string? licenseData { get; set; }
    public object? isInvalid { get; set; }
    public DateTime? createdAt { get; set; }
}

public class Root
{
    public int? code { get; set; }
    public Data? data { get; set; }
    public object? msg { get; set; }
    public long? time { get; set; }
}
