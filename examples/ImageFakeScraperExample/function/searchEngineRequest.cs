namespace ImageFakeScraperExample.function;

public class searchEngineRequest
{
    #region Var
    private static List<string> googleResult = new();
    private static readonly GoogleScraper scraper = new();

    private static List<string> ducResult = new();
    private static readonly DuckDuckGoScraper duck = new();

    private static List<string> BraveResult = new();
    private static readonly BraveScraper brave = new();

    private static List<string> OpenResult = new();
    private static readonly OpenVerseScraper open = new();

    private static List<string> BingResult = new();
    private static readonly BinImageFakeScraper bingg = new();

    private static List<string> YahooResult = new();
    private static readonly YahooScraper yahooo = new();

    private static List<string> GettyResult = new();
    private static readonly GettyScraper Gettyy = new();

    private static List<string> EveryResult = new();
    private static readonly PixelScraper pixell = new();

    private static List<string> immerseResult = new();
    private static readonly ImmerseScraper immerse = new();

    private static readonly Dictionary<string, List<string>> returnLink = new();

    public static int NbOfRequest = 0;
    #endregion

    #region getAllDataFromsearchEngineAsync
    public static async Task<Dictionary<string, List<string>>> getAllDataFromsearchEngineAsync(string text)
    {
        returnLink.Clear();
        #region Google
        if (Settings.GoogleRun)
        {
            googleResult.Clear();
            try
            {
                googleResult = await scraper.GetImagesAsync(text);
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("Google", googleResult);

        }
        #endregion
        #region DuckduckGO
        if (Settings.DuckduckGORun)
        {
            ducResult.Clear();
            try
            {
                ducResult = await duck.GetImagesAsync(text);
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("DuckDuckGo", ducResult);

        }
        #endregion
        #region Brave
        if (Settings.BraveRun)
        {
            BraveResult.Clear();
            try
            {
                BraveResult = await brave.GetImagesAsync(text);
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("Brave", BraveResult);
        }
        #endregion
        #region OpenVerse
        if (Settings.OpenVerseRun)
        {
            OpenResult.Clear();
            try
            {
                OpenResult = await open.GtImagesAsync(text);
            }
            catch { }
            NbOfRequest += open.NbOfRequest;
            returnLink.Add("Open", OpenResult);
        }
        #endregion
        #region Bing
        if (Settings.BingRun)
        {
            BingResult.Clear();
            try
            {
                BingResult = await bingg.GetImagesAsync(text);
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("Bing", BingResult);

        }
        #endregion
        #region Yahoo
        if (Settings.YahooRun)
        {
            YahooResult.Clear();
            try
            {
                YahooResult = await yahooo.GetImagesAsync(text);
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("Yahoo", YahooResult);
        }
        #endregion
        #region GettyImage
        if (Settings.GettyImageRun)
        {
            GettyResult.Clear();
            try
            {
                GettyResult = await Gettyy.GetImagesAsync(text);
            }
            catch { }
            NbOfRequest += Gettyy.NbOfRequest;
            returnLink.Add("Getty", GettyResult);
        }
        #endregion
        if (Settings.ImmerseRun)
        {
            immerseResult.Clear();
            try
            {
                immerseResult = await immerse.GetImagesAsync(text);
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("Immerse", immerseResult);
        }
        #region EveryPixel
        if (Settings.EveryPixelRun)
        {
            EveryResult.Clear();
            try
            {
                EveryResult = await pixell.GetImagesAsync(text);
            }
            catch { }
            NbOfRequest += pixell.NbOfRequest;
            returnLink.Add("Pixel", EveryResult);
        }
        #endregion

        return returnLink;
    }
    #endregion

    #region getAllNextTag
    //public static async Task<Queue<string>> getAllNextTag(string text, IDatabase redis)
    //{
    //
    //    string url = $"https://www.google.com/search?q={text}&tbm=isch&hl=en";
    //    using (HttpClient client = new())
    //    {
    //        try
    //        {
    //            using HttpResponseMessage response = client.GetAsync(url).Result;
    //            using HttpContent content = response.Content;
    //            string result = content.ReadAsStringAsync().Result;
    //            HtmlDocument document = new();
    //            document.LoadHtml(result);
    //
    //            table = document.DocumentNode.SelectNodes("//a[@class='TwVfHd']");
    //        }
    //        catch { }
    //
    //        try
    //        {
    //            if (table != null)
    //            {
    //                for (int j = 0; j < table.Count; j++)
    //                {
    //                    if (await Read(redis, table[j].InnerText) == -1)
    //                    {
    //                        if (addNewTag_Bing_Google || addNewTag_Google)
    //                            qword.Enqueue(table[j].InnerText);
    //
    //                        //Console.ForegroundColor = ConsoleColor.Green;
    //                        //await Console.Out.WriteLineAsync($"Tag Added {table[j].InnerText}");
    //                        //Console.ResetColor();
    //                    }
    //                    else
    //                    {
    //                        //Console.ForegroundColor = ConsoleColor.Red;
    //                        //await Console.Out.WriteLineAsync($"Tag already exist {table[j].InnerText}");
    //                        //Console.ResetColor();
    //                    }
    //                }
    //                NbOfRequest++;
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Console.ForegroundColor = ConsoleColor.Red;
    //            await Console.Out.WriteLineAsync("No tag found!");
    //            Console.WriteLine(e);
    //            Console.ResetColor();
    //        }
    //    }
    //
    //    return qword;
    //}
    #endregion

    #region redisRead
    private static async Task<long> Read(IDatabase redis, string text)
    {
        return await redis.ListPositionAsync("words_done", text);
    }
    #endregion
}
