using Newtonsoft.Json;
#pragma warning disable CS8602, CS8604, CS8618, CS1634
namespace ImageFakeScraper.UnsplashNgetty
{
	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	public class DisplaySize
	{
		public string uri { get; set; }
	}

	public class Image
	{
		public List<DisplaySize> display_sizes { get; set; }
	}

	public class Root
	{
		public List<Image> images { get; set; }
	}

}

