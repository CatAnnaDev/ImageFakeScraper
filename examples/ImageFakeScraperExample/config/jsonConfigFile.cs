#pragma warning disable
namespace ImageFakeScraperExample.config
{
	internal class jsonConfigFile
	{
		public string? Credential { get; set; }
		public int nbThread { get; set; }
		public int requestMaxPerSec { get; set; }

		public int QueueLimit { get; set; }
		public string? words_list { get; set; }
		public string? images_jobs { get; set; }
		public string? to_download { get; set; }
		public Settings? settings { get; set; }
		public Engines? Engines { get; set; }
		public SettingsDll settingsDLL { get; set; }

	}
}
