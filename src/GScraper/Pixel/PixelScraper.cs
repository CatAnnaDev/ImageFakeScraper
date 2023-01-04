using Newtonsoft.Json;
namespace ImageFakeScraper.Pixel;

public class PixelScraper
{
    public PixelScraper()
    {
    }

    private List<string> tmp = new();
    public int NbOfRequest = 0;
    private const string uri = "https://www.everypixel.com/search/search?q={0}&limit=20000&json=1&page={1}";
    private readonly Regex RegexCheck = new(@"^(http|https:\/\/):?([^\s([<,>\/]*)(\/)[^\s[,><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s[,><]*)?");

    public async Task<List<string>> GetImagesAsync(string query)
    {
        try
        {
            tmp.Clear();
            NbOfRequest = 0;
            ImageFakeScraperGuards.NotNull(query, nameof(query));


            for (int i = 1; i < 500; i++)
            {
                string[] args = new string[] { query, i.ToString() };
                string jsonGet = await httpRequest.GetJson(uri, args);
                Root jsonparsed = JsonConvert.DeserializeObject<Root>(jsonGet);
                if (jsonparsed != null)
                {
                    if (jsonparsed.images != null)
                    {
                        if (jsonparsed.images.images_0 != null)
                        {
                            for (int j = 0; j < jsonparsed.images.images_0.Count; j++)
                            {
                                if (RegexCheck.IsMatch(jsonparsed.images.images_0[i].url))
                                {
                                    tmp.Add(jsonparsed.images.images_0[j].url);
                                }
                            }
                        }
                    }
                    else
                        break;
                }
                else
                    break;
                NbOfRequest++;
            }
        }
        catch (Exception e) { }
        return tmp;
    }
}