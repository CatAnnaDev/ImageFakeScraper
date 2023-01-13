using Newtonsoft.Json;

namespace ImageFakeScraper.Alamy
{
	public class AlamyScraper
	{
		public AlamyScraper()
		{
		}

        private readonly List<string> tmp = new();
        private const string uri = "https://www.alamy.com/search-api/search/?qt={0}&sortBy=relevant&ispartial=false&type=picture&geo=FR&pn={1}&ps={2}"; // qt query, pn page numb, ps page size
        public int NbOfRequest = 0;
        private readonly Regex RegexCheck = new(@"^(https:\/\/)?s?:?([^\s([""<,>\/]*)(\/)[^\s["",><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s["",><]*)?");

        public async Task<List<string>> GtImagesAsync(string query, int AlamyMaxPage, int AlamyMaxResult, bool UnlimitedCrawlPage)
        {
            try
            {
                tmp.Clear();
                NbOfRequest = 0;
                int page = AlamyMaxPage+1;
                ImageFakeScraperGuards.NotNull(query, nameof(query));
                for (int i = 1; i < page; i++)
                {
                    string[] args = new string[] { query, i.ToString(), AlamyMaxResult.ToString() };
                    string jsonGet = await httpRequest.GetJson(uri, args);
                    Root jsonparsed = JsonConvert.DeserializeObject<Root>(jsonGet);
                    if (jsonparsed != null)
                    {
                        if (jsonparsed.Items != null)
                        {
                            if (jsonparsed.Items.Count != 0)
                            {
                                for (int j = 0; j < jsonparsed.Items.Count; j++)
                                {
                                    if (RegexCheck.IsMatch(jsonparsed.Items[j].Renditions.Comp.Href))
                                    {
                                        tmp.Add(jsonparsed.Items[j].Renditions.Comp.Href);
                                    }
                                }
                                if(UnlimitedCrawlPage)
                                    page++;
                            }
                            else
                                break;
                        }
                    }
                    else
                        break;
                    NbOfRequest++;
                }

            }
            catch { }
            return tmp;
        }
    }
}

