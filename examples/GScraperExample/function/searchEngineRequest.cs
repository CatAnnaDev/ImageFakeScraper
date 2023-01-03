using GScraper.OpenVerse;

namespace GScraperExample.function;

public class searchEngineRequest
{
    #region Var
    private static readonly List<string> googleResult = new();
    private static readonly GoogleScraper scraper = new();

    private static readonly List<string> ducResult = new();
    private static readonly DuckDuckGoScraper duck = new();
    private static bool ddc = false;

    private static readonly List<string> BraveResult = new();
    private static readonly BraveScraper brave = new();
    private static bool brv = false;

    private static readonly List<string> OpenResult = new();
    private static readonly OpenVerseScraper open = new();
    private static bool ov = true;

    private static readonly List<string> BingResult = new();
    private static readonly BingScraper bingg = new();
    private static bool bing = true;

    private static readonly List<string> YahooResult = new();
    private static readonly YahooScraper yahooo = new();
    private static bool yahoo = true;

    private static readonly List<string> GettyResult = new();
    private static readonly GettyScraper Gettyy = new();
    private static bool getty = true;

    private static readonly List<string> EveryResult = new();
    private static readonly PixelScraper pixell = new();
    private static bool every = true;

    private static readonly List<string> immerseResult = new();
    private static readonly ImmerseScraper immerse = new();
    private static bool imme = true;

    private static readonly Dictionary<string, List<string>> returnLink = new();

    public static int NbOfRequest = 0;
    #endregion

    #region getAllDataFromsearchEngineAsync
    public static async Task<Dictionary<string, List<string>>> getAllDataFromsearchEngineAsync(string text)
    {
        returnLink.Clear();
        #region Google
        if (GoogleScraper.gg)
        {
            googleResult.Clear();
            try
            {
                var google = await scraper.GetImagesAsync(text);
                if (google.Count > 0)
                    google.ForEach(image => { googleResult.Add(image); });
            }
            catch{}
            NbOfRequest++;
            returnLink.Add("Google", googleResult);
            
        }
        #endregion
        #region DuckduckGO
        if (ddc)
        {
            ducResult.Clear();
            try
            {
                var duckduck = await duck.GetImagesAsync(text);
                if (duckduck.Count > 0)
                    duckduck.ForEach(image => { ducResult.Add(image); });
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("DuckDuckGo", ducResult);
            
        }
        #endregion
        #region Brave
        if (brv)
        {
            BraveResult.Clear();
            try
            {
                var bravelist = await brave.GetImagesAsync(text);
                if (bravelist.Count > 0)
                    bravelist.ForEach(image => { BraveResult.Add(image); });
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("Brave", BraveResult);
        }
        #endregion
        #region OpenVerse
        if (ov)
        {
            OpenResult.Clear();
            try
            {
                var openresult = await open.GtImagesAsync(text);
                if (openresult.Count > 0)
                    openresult.ForEach(image => { OpenResult.Add(image); });
            }
            catch { }
            NbOfRequest += open.NbOfRequest;
            returnLink.Add("Open", OpenResult);
        }
        #endregion
        #region Bing
        if (bing)
        {
            BingResult.Clear();
            try
            {
                var bingResult = await bingg.GetImagesAsync(text);
                if (bingResult.Count > 0)
                    bingResult.ForEach(image => { BingResult.Add(image); });
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("Bing", BingResult);

        }
        #endregion
        #region Yahoo
        if (yahoo)
        {
            YahooResult.Clear();
            try
            {
                var yahooResult = await yahooo.GetImagesAsync(text);
                if (yahooResult.Count > 0)
                    yahooResult.ForEach(image => { YahooResult.Add(image); });
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("Yahoo", YahooResult);
        }
        #endregion
        #region GettyImage
        if (getty)
        {
            GettyResult.Clear();
            try
            {
                var gettyResult = await Gettyy.GetImagesAsync(text);
                if (gettyResult.Count > 0)
                    gettyResult.ForEach(image => { GettyResult.Add(image); });
            }
            catch { }
            NbOfRequest += Gettyy.NbOfRequest;
            returnLink.Add("Getty", GettyResult);
        }
        #endregion
        if (imme)
        {
            immerseResult.Clear();
            try
            {
                var gettyResult = await immerse.GetImagesAsync(text);
                if (gettyResult.Count > 0)
                    gettyResult.ForEach(image => { immerseResult.Add(image); });
            }
            catch { }
            NbOfRequest++;
            returnLink.Add("Immerse", immerseResult);
        }
        #region EveryPixel
        if (every)
        {
            EveryResult.Clear();
            try
            {
                var pixelResult = await pixell.GetImagesAsync(text);
                if (pixelResult.Count > 0)
                    pixelResult.ForEach(image => { EveryResult.Add(image); });
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
