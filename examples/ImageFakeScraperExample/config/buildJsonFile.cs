using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImageFakeScraperExample.config
{
	internal class buildJsonFile
	{
		public string ConfigPath { get; set; } = "Config.json";
		public jsonConfigFile? Config { get; set; }
		public JObject? Configs { get; set; }

		public async Task InitializeAsync()
		{
			string jsonUser;
			string jsonDefault = JsonConvert.SerializeObject(GenerateNewConfig(), Formatting.Indented);
			if (File.Exists(ConfigPath))
			{
				jsonUser = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
				Config = JsonConvert.DeserializeObject<jsonConfigFile>(jsonUser);

				JObject userConf = JObject.Parse(jsonUser);

				JObject defaultConf = JObject.Parse(jsonDefault);

				Configs = new JObject();

				Configs.Merge(defaultConf);
				Configs.Merge(userConf);
			}
			else
			{
				File.WriteAllText("Config.json", jsonDefault, new UTF8Encoding(false));
				Console.WriteLine($"Update config file \n{Directory.GetCurrentDirectory()}\\Config.json");
				await Task.Delay(-1);
			}

			jsonUser = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
			Config = JsonConvert.DeserializeObject<jsonConfigFile>(jsonUser);

			File.WriteAllText("Config.json", Configs.ToString(), new UTF8Encoding(false));
		}

		private jsonConfigFile GenerateNewConfig()
		{
			return new jsonConfigFile
			{
				Credential = "Redis Login",
				requestMaxPerSec = 8,
				nbThread = 4,
				words_list = "words_list",
				images_jobs = "image_jobs",
				to_download = "to_download",
				Engines = new()
				{
					Google = false,
					UnsNapi = false,
					UnsNge = false,
					Qwant = false,
					Shutter = false,
					Alamy = false,
					Open = false,
					Bing = false,
					Yahoo = false,
					Getty = false,
					Immerse = false,
					Pixel = false,
				},
				settings = new()
				{
					stopAfter = 8000000,
					PrintLogTag = true,
					PrintLog = true
				},
				settingsDLL = new()
				{
					printLog = false,
					printErrorLog = false
				}
			};
		}
	}
}