using Newtonsoft.Json;

namespace ImageFakeScraper.Pixel;


public class Images
{
	public List<Images0>? images_0 { get; set; }
}

public class Images0
{
	public string? url { get; set; }
}

public class Root
{
	public Images? images { get; set; }
}

