using System.Threading;

namespace ImageFakeScraper
{
    public class Scraper
    {
        protected IDatabase redis;
        protected Dictionary<string, object> Options;
        protected SimpleMovingAverage movingAverage;
		public httpRequest http = new();

        public virtual async Task<int> GetImages(AsyncCallback ac, params object[] args) { return 0; }

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

        public virtual void setRedis(IDatabase redis)
        {
            this.redis = redis;
        }

        public virtual void setOptions(Dictionary<string, object> key)
        {
            Options = key;
        }

        public virtual void setMovingAverage(SimpleMovingAverage movingAverage)
        {
            this.movingAverage = movingAverage;
        }
    }
}