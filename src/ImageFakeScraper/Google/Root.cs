#pragma warning disable CS8602, CS8604, CS8618, CS1634
namespace ImageFakeScraper.Google
{

	public class Ischj
	{
		public List<Metadata> metadata { get; set; }
	}

	public class Metadata
	{
		public OriginalImage original_image { get; set; }
	}

	public class OriginalImage
	{
		public string url { get; set; }
	}

	public class Root
	{
		public Ischj ischj { get; set; }
	}


}
