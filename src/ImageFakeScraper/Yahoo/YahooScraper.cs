namespace ImageFakeScraper.Yahoo;
public class YahooScraper : Scraper
{

    public YahooScraper()
    {

    }

    private readonly List<string> tmp = new();
    private const string uri = "https://images.search.yahoo.com/search/images?ei=UTF-8&p={0}&fr2=p%3As%2Cv%3Ai&.bcrumb=4N2SA8f4BZT&save=0";
    private readonly Regex RegexCheck = new(@"^(http|https:):?([^\s([<,>]*)(\/)[^\s[,><]*(\?[^\s[,><]*)?");

    public async Task<List<string>> GetImagesAsync(string query)
    {
        try
        {
            tmp.Clear();
            ImageFakeScraperGuards.NotNull(query, nameof(query));
            string[] args = new string[] { query };
            HtmlDocument doc = await httpRequest.Get(uri, args);
            IEnumerable<string> urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("data-src", null)).Where(s => !string.IsNullOrEmpty(s));

            foreach (string? data in urls)
            {
                if (RegexCheck.IsMatch(data))
                {
                    string cleanUrl = Regex.Replace(data, @"&pid=Api&P=0&w=300&h=300", "");
                    if (!cleanUrl.EndsWith("th") && !data.Contains("th?id="))
                    {
                        tmp.Add(cleanUrl);
                    }
                }
            }
        }
        catch (Exception e) {  }
        return tmp;
    }

    public override async Task<List<string>> GetImages(params object[] args)
    {
        return await GetImagesAsync((string)args[0]);
    }
}