namespace ImageFakeScraper.Bing;


public class BinImageFakeScraper : Scraper
{

    public BinImageFakeScraper()
    {

    }

    private readonly List<string> tmp = new();
    private const string uri = "https://www.bing.com/images/search?q={0}&ghsh=0&ghacc=0&first=1&tsc=ImageHoverTitle&adlt=off";
    private readonly Regex RegexCheck = new(@"^(http|https:):?([^\s([<,>]*)(\/)[^\s[,><]*(\?[^\s[,><]*)?");

    // (https:\/\/)?s?:?([^\s(["<,>/]*)(\/)[^\s[",><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s[",><]*)?

    // Regex.Match(url, @"^(?:https?://)?(?:[^@/\n]+@)?(?:www.)?([^:/?\n]+)").Groups[1].Value


    public async Task<List<string>> GetImagesAsync(string query)
    {
        try
        {
			tmp.Clear();
            ImageFakeScraperGuards.NotNull(query, nameof(query));
            string[] args = new string[] { query };
            HtmlDocument doc = await http.Get(uri, args);
            IEnumerable<string> urls = doc.DocumentNode.Descendants("img").Select(e => e.GetAttributeValue("src", null)).Where(s => !string.IsNullOrEmpty(s));
            
            //HtmlNodeCollection tag = doc.DocumentNode.SelectNodes("//div[@class='suggestion-title-wrapper']");

            //if (tag != null)
            //{
            //    foreach (HtmlNode? item in tag)
            //    {
            //        if (addNewTag_Bing_Google || addNewTag_Bing)
            //        {
            //            if (await Read(redisConnection.GetDatabase, item.FirstChild.InnerText) == -1)
            //            {
            //                if (!qword.Contains(item.FirstChild.InnerText))
            //                    qword.Enqueue(item.FirstChild.InnerText);
            //            }
            //        }
            //    }
            //}

            for (int i = 0; i<urls.Count(); i++)
            {
                if (RegexCheck.IsMatch(urls.ElementAt(i)))
                {
                    string cleanUrl = Regex.Replace(urls.ElementAt(i), @"[?&][^?&]+=[^?&]+", "");
                    if (!cleanUrl.EndsWith("th") && !urls.ElementAt(i).Contains("th?id="))
                    {
                        tmp.Add(cleanUrl);
                    }
                }
            }
        }
        catch (Exception e) { Console.WriteLine("Bing " + e); }
        return tmp;
    }

    public override async Task<List<string>> GetImages(params object[] args)
    {
        return await GetImagesAsync((string)args[0]);
    }
}