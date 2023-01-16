using System.Threading;

namespace ImageFakeScraper
{
    public class Scraper
    {
        protected IDatabase redis;
        protected string RedisPushKey = "";
        public Scraper(IDatabase redis, string key) 
        { 
            this.redis = redis;
            RedisPushKey = key;

		}
		public httpRequest http = new();

		public virtual async void GetImages(AsyncCallback ac, params object[] args) { }


    }
}