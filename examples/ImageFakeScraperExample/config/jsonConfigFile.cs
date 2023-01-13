namespace ImageFakeScraperExample.config
{
    internal class jsonConfigFile
    {
        public string? Credential { get; set; }
        public double Sleep { get; set; }
        public int Thread { get; set; }
        public int QueueLimit { get; set; }
        public string? words_list { get; set; }
        public string? images_jobs { get; set; }
        public string? to_download { get; set; }
        public Settings? settings { get; set; }

    }
}
