namespace ImageFakeScraper.immerse;

public class Data
{
	public List<ImageDatum>? imageData { get; set; }
}

public class ImageDatum
{
	public string? sourceImageUrl { get; set; }
}

public class Root
{
	public Data? data { get; set; }
}
