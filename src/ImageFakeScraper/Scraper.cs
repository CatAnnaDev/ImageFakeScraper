using System.Threading;

namespace ImageFakeScraper
{
    public class Scraper
    {
        protected IDatabase redis;
        protected Dictionary<string, object> Options = new();
        public Scraper(IDatabase redis, Dictionary<string, object> key) 
        { 
            this.redis = redis;
            Options = key;

		}
		public httpRequest http = new();

		public virtual async void GetImages(AsyncCallback ac, params object[] args) { }


        public async Task<bool> redisCheckCount()
        {
			RedisValue to_dl = await redis.SetLengthAsync(Options["redis_queue_limit_name"].ToString());
			int to_dl_count = int.Parse(to_dl.ToString());
			if (to_dl_count >= int.Parse(Options["redis_queue_limit_count"].ToString()))
			{
                Console.Write("\rRedis FULL !!!");
				return false;
			}
            return true;
		}

    }
}