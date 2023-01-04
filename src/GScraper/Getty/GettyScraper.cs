namespace GScraper.Getty;

public class GettyScraper
{
    public GettyScraper()
    {
    }

    private List<string> tmp = new();
    private const string uri = "https://www.gettyimages.fr/photos/{0}?assettype=image&excludenudity=false&license=rf&family=creative&phrase={1}&sort=mostpopular&page={2}";
    public int NbOfRequest = 0;
    private readonly Regex RegexCheck = new(@"^(https:\/\/)?s?:?([^\s([""<,>\/]*)(\/)[^\s["",><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s["",><]*)?");

    /// <summary>
    /// GetImagesAsync for Getty
    /// </summary>
    /// <param name="query">string param</param>
    /// <returns></returns>
    public async Task<List<string>> GetImagesAsync(string query)
    {
        try
        {
            tmp.Clear();
            NbOfRequest = 0;
            GScraperGuards.NotNull(query, nameof(query));
            for (int i = 1; i < 500; i++)
            {
                object[] args = new object[] {query, query, i.ToString()};
                var doc = await httpRequest.Get(uri, args);
                IEnumerable<string> urls = doc.DocumentNode.Descendants("source").Select(e => e.GetAttributeValue("srcSet", null)).Where(s => !String.IsNullOrEmpty(s));

                if (urls.Count() == 0)
                    break;

                foreach (string? data in urls)
                {
                    if (RegexCheck.IsMatch(data))
                    {
                        tmp.Add(data.Replace("&amp;", "&"));
                    }
                }
                NbOfRequest++;
            }
        }
        catch (Exception e) { Console.WriteLine(e);}
        return tmp;
    }
}