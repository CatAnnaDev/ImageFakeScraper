using Newtonsoft.Json;
namespace ImageFakeScraper.OpenVerse;

public class OpenVerseScraper
{
    public OpenVerseScraper() { }

    private List<string> tmp = new();

    private const string uri = "https://api.openverse.engineering/v1/images/?format=json&q={0}&page={1}&mature=true";
    private readonly Regex RegexCheck = new(@"^(http|https://):?([^\s([<,>/]*)(\/)[^\s[,><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s[,><]*)?");
    public int NbOfRequest = 0;

    public async Task<List<string>> GtImagesAsync(string query)
    {
        try
        {
            tmp.Clear();
            NbOfRequest = 0;
            int page = Settings.OpenVerseMaxPage + 1;
            ImageFakeScraperGuards.NotNull(query, nameof(query));
            for (int i = 1; i < page; i++)
            {
                string[] args = new string[] { query, i.ToString() };
                string jsonGet = await httpRequest.GetJson(uri, args);
                Root jsonparsed = JsonConvert.DeserializeObject<Root>(jsonGet);
                if (jsonparsed != null)
                {
                    page = jsonparsed.page_count;
                    if (jsonparsed.results != null)
                    {
                        for (int j = 0; j < jsonparsed.results.Count; j++)
                        {
                            if (RegexCheck.IsMatch(jsonparsed.results[i].url))
                            {
                                tmp.Add(jsonparsed.results[j].url);
                            }
                        }
                    }
                }
                NbOfRequest++;
            }

        }
        catch { }
        return tmp;
    }
}
