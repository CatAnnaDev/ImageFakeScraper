using Newtonsoft.Json;
#pragma warning disable CS8602, CS8604, CS8618, CS1634
namespace ImageFakeScraper.Alamy
{

	public class Comp
	{
		public string Href { get; set; }
	}

	public class Item
	{
		public Renditions Renditions { get; set; }

	}

	public class Renditions
	{
		public Comp Comp { get; set; }

	}

	public class Root
	{
		public List<Item> Items { get; set; }
	}


}

