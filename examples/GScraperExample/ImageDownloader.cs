using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace GScraperExample
{
    public class ImageDownloader
    {
        public static void DownloadImagesFromUrl(string url, string folderImagesPath = "")
        {
            var uri = new Uri(url);
            var pages = new List<HtmlNode> { LoadHtmlDocument(uri) };
            for (int i = 0; i < pages.Count; i++)
            {
                pages.AddRange(LoadOtherPages(pages[i], url));

                pages.SelectMany(p => p.Descendants("img"))
                    .Select(node => Tuple.Create(new UriBuilder(uri.Scheme, uri.Host, uri.Port, node.Attributes["src"].Value).Uri, new WebClient()))
                    .AsParallel()
                    .ForAll(t => DownloadImage(folderImagesPath, t.Item1, t.Item2));
            }
        }

        private static void DownloadImage(string folderImagesPath, Uri url, WebClient webClient)
        {
            Console.WriteLine(url);
        }

        private static IEnumerable<HtmlNode> LoadOtherPages(HtmlNode firstPage, string url)
        {
            return Enumerable.Range(1, Extract(firstPage)).AsParallel().Select(i => LoadHtmlDocument(new Uri(url)));
        }

        public static int Extract(HtmlNode html)
        {
            List<string> list = new List<string>();

            Regex regex = new Regex("(?:href|src)=[\"|']?(.*?)[\"|'|>]+", RegexOptions.Singleline | RegexOptions.CultureInvariant);
            if (regex.IsMatch(html.InnerHtml))
            {
                foreach (Match match in regex.Matches(html.InnerHtml))
                {
                    list.Add(match.Groups[1].Value);
                }
            }
            var totalPages = (int)Math.Ceiling(list.Count / 50d);
            return totalPages;
        }

        private static HtmlNode LoadHtmlDocument(Uri uri)
        {
            var doc = new HtmlDocument();
            var wc = new WebClient();
            doc.LoadHtml(wc.DownloadString(uri));
            var documentNode = doc.DocumentNode;
            return documentNode;
        }
    }
}
