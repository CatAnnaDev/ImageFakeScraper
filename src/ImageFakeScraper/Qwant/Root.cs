#pragma warning disable CS8602, CS8604, CS8618, CS1634
namespace ImageFakeScraper.Qwant
{

	public class Data
	{
		public Result result { get; set; }
	}

	public class Item
	{
		public string media { get; set; }

	}

	public class Result
	{
		public List<Item> items { get; set; }
	}

	public class Root
	{
		public Data data { get; set; }
	}
}

