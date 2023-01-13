namespace ImageFakeScraperExample.config
{
    internal class jsonConfigFile
    {
        public string? Credential { get; set; }
        public double Sleep { get; set; }
        public string? Pseudo { get; set; }
        public string? domain_blacklist { get; set; }
        public string? words_list { get; set; }
        public string? words_done { get; set; }
        public string? images_jobs { get; set; }
        public string? to_download { get; set; }
        public Settings? settings { get; set; }
        public SettingsDll? settingsDll { get; set; }
    }
}
