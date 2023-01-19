using Newtonsoft.Json;
#pragma warning disable CS8602, CS8604, CS8618, CS1634
namespace ImageFakeScraper.UnsplashNapi
{
	public class Result
	{
		public Urls urls { get; set; }
	}

	public class Root
	{
		public List<Result> results { get; set; }
	}


	public class Urls
	{
		public string raw { get; set; }
	}

}

