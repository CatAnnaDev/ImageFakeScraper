using Reddit;
using Reddit.Controllers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GScraperExample
{
    static class Reddit
    {

        public static void RedditCrawler(ConnectionMultiplexer connection)
        {
            string redditfile = "subredditlist.txt";
            List<string> subreddits = new List<string> { };
            string[] file = File.ReadAllLines(redditfile);
            file.Reverse();
            foreach (string sr in file) subreddits.Add(sr.Replace("/r/", "").Trim());

            RedditClient r = new RedditClient(appId: "egFSCSCdn8l1Zq1KUNJ7Vw", appSecret: "hSYyoV3IXKfvhSoPhxXRH8I3VgnrEg", refreshToken: "301142678066-jiF1H0nXuY3-SkOWRCtI3zaTyXcIqg");

            List<(string url, string subreddit)> Urls = GrabPosts(r, subreddits);
            Download(Urls, connection);
        }

        private static void Download(List<(string url, string subreddit)> urls, ConnectionMultiplexer connection)
        {
            Parallel.ForEach(urls, (url) =>
            {
                Console.WriteLine($" Done Downloading: {url.url}");
                connection.GetDatabase().SetAdd("image_jobs", url.url);
            });


        }

        private static List<(string url, string subreddit)> GrabPosts(RedditClient r, List<string> subreddits)
        {

            Regex rx = new Regex(@".*\.(jpg|png|gif)?$");
            List<(string, string)> Url = new List<(string, string)> { };
            Parallel.ForEach(subreddits, (subreddit) =>
            {
                try
                {
                    Console.WriteLine(subreddit);
                    SubredditPosts subs = r.Subreddit(subreddit).Posts;

                    List<Post> Posts = subs.Top;

                    foreach (Post post in Posts)
                    {
                        string url = post.Listing.URL;
                        if (rx.IsMatch(url))
                        {
                            Url.Add((url, subreddit));
                        }
                    }

                }
                catch (Exception e) { Console.WriteLine(e.Message); }
            });
            return Url;

        }
    }
}
