﻿namespace ImageFakeScraper.Getty;

public class GettyScraper : Scraper
{
    public GettyScraper()
    {
    }

    private readonly List<string> tmp = new();
    private const string uri = "https://www.gettyimages.fr/photos/{0}?assettype=image&excludenudity=false&license=rf&family=creative&phrase={1}&sort=mostpopular&page={2}";
    private readonly Regex RegexCheck = new(@"^(https:\/\/)?s?:?([^\s([""<,>\/]*)(\/)[^\s["",><]*(.png|.jpg|.jpeg|.gif|.avif|.webp)(\?[^\s["",><]*)?");

    /// <summary>
    /// GetImagesAsync for Getty
    /// </summary>
    /// <param name="query">string param</param>
    /// <returns></returns>
    public async Task<List<string>> GetImagesAsync(string query, int GettyMaxPage)
    {
        try
        {
            tmp.Clear();
            ImageFakeScraperGuards.NotNull(query, nameof(query));
            for (int i = 1; i < GettyMaxPage + 1; i++)
            {
                object[] args = new object[] { query, query, i.ToString() };
                HtmlDocument doc = await http.Get(uri, args);
                IEnumerable<string> urls = doc.DocumentNode.Descendants("source").Select(e => e.GetAttributeValue("srcSet", null)).Where(s => !string.IsNullOrEmpty(s));

                if (urls.Count() == 0)
                {
                    break;
                }

                for (int j = 0; j < urls.Count(); j++)
                {
                    if (RegexCheck.IsMatch(urls.ElementAt(j)))
                    {
                        tmp.Add(urls.ElementAt(j).Replace("&amp;", "&"));
                    }
                }
            }
        }
        catch (Exception e) { Console.WriteLine("Getty " + e); }
        return tmp;
    }

    public override async Task<List<string>> GetImages(params object[] args)
    {
        return await GetImagesAsync((string)args[0], (int)args[1]);
    }
}