namespace ImageFakeScraperExample.uselessCode
{
    public class ImageDownloader
    {
        public static void DownloadImagesFromUrl(string url, string folderImagesPath = "")
        {
            Uri uri = new(url);
            List<HtmlNode> pages = new() { LoadHtmlDocument(uri) };
            pages.AddRange(LoadOtherPages(pages[0], url));
            //Console.WriteLine(pages[i].);
            //pages.SelectMany(p => p.Descendants("img")).Select(node => node.Attributes["src"]).AsParallel().ForAll(t => DownloadImage(t));

            foreach (HtmlAttribute? link in pages.SelectMany(p => p.Descendants("img").Select(i => i.Attributes["src"])).AsParallel())
            {
                Console.WriteLine(link.Value.ToString());
            }
        }

        private static void DownloadImage(Uri url)
        {
            Console.WriteLine(url);
        }

        private static IEnumerable<HtmlNode> LoadOtherPages(HtmlNode firstPage, string url)
        {
            return Enumerable.Range(1, Extract(firstPage)).AsParallel().Select(i => LoadHtmlDocument(new Uri(url)));
        }

        public static int Extract(HtmlNode html)
        {
            List<string> list = new();

            Regex regex = new("(?:href|src)=[\"|']?(.*?)[\"|'|>]+", RegexOptions.Singleline | RegexOptions.CultureInvariant);
            if (regex.IsMatch(html.InnerHtml))
            {
                foreach (Match match in regex.Matches(html.InnerHtml))
                {
                    list.Add(match.Groups[1].Value);
                }
            }
            int totalPages = (int)Math.Ceiling(list.Count / 50d);
            return totalPages;
        }

        private static HtmlNode LoadHtmlDocument(Uri uri)
        {
            HtmlDocument doc = new();
            WebClient wc = new();
            doc.LoadHtml(wc.DownloadString(uri));
            HtmlNode documentNode = doc.DocumentNode;
            return documentNode;
        }
    }
}
