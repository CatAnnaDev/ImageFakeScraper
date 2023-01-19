using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFakeScraper.Shutterstock
{
	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);


	public class Asset
	{
		public string src { get; set; }
	}

	public class PageProps
	{
		public List<Asset> assets { get; set; }

	}

	public class Root
	{
		public PageProps pageProps { get; set; }

	}



}
