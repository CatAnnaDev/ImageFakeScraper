namespace GScraperExample.uselessCode;

internal static class Reddit
{

    public static void RedditCrawler(ConnectionMultiplexer connection)
    {
        string redditfile = "subredditlist.txt";
        List<string> subreddits = new() { };
        string[] file = File.ReadAllLines(redditfile);
        _ = file.Reverse();
        foreach (string sr in file)
        {
            subreddits.Add(sr.Replace("/r/", "").Trim());
        }

        RedditClient r = new(appId: "egFSCSCdn8l1Zq1KUNJ7Vw", appSecret: "hSYyoV3IXKfvhSoPhxXRH8I3VgnrEg", refreshToken: "301142678066-jiF1H0nXuY3-SkOWRCtI3zaTyXcIqg");

        List<(string url, string subreddit)> Urls = GrabPosts(r, subreddits);
        Download(Urls, connection);
    }

    private static void Download(List<(string url, string subreddit)> urls, ConnectionMultiplexer connection)
    {
        _ = Parallel.ForEach(urls, (url) =>
        {
            Console.WriteLine($" Done Downloading: {url.url}");
            _ = connection.GetDatabase().SetAdd("image_jobs", url.url);
        });


    }

    private static List<(string url, string subreddit)> GrabPosts(RedditClient r, List<string> subreddits)
    {

        Regex rx = new(@".*\.(jpg|png|gif)?$");
        List<(string, string)> Url = new() { };
        _ = Parallel.ForEach(subreddits, (subreddit) =>
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
            catch (Exception e) { Console.WriteLine(e); }
        });
        return Url;

    }
}
