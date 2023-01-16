namespace ImageFakeScraper
{
    public class Scraper
    {
		public httpRequest http = new();

		public virtual async Task<List<string>> GetImages(params object[] args) { return new List<string>(); }

    }
}