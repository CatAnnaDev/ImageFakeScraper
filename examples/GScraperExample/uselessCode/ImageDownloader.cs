using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace GScraperExample.uselessCode
{
    public class ImageDownloader
    {
        public static void DownloadImagesFromUrl(string url, string folderImagesPath = "")
        {
            var uri = new Uri(url);
            var pages = new List<HtmlNode> { LoadHtmlDocument(uri) };
            pages.AddRange(LoadOtherPages(pages[0], url));
            //Console.WriteLine(pages[i].);
            //pages.SelectMany(p => p.Descendants("img")).Select(node => node.Attributes["src"]).AsParallel().ForAll(t => DownloadImage(t));

            foreach (var link in pages.SelectMany(p => p.Descendants("img").Select(i => i.Attributes["src"])).AsParallel())
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
